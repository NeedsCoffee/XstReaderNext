// Copyright (c) 2016, Dijji, and released under Ms-PL.  This can be found in the root of this distribution. 

using SearchTextBox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace XstReader
{
    /// <summary>
    /// XstReader is a viewer for xst (.ost and .pst) files
    /// This file contains the interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private View view = new View();
        private readonly MailboxSessionService mailboxSessionService = new MailboxSessionService(new XstFileMailboxSessionOperations());
        private List<string> tempFileNames = new List<string>();
        private readonly AppSettings settings = AppSettings.Load();
        private int searchIndex = -1;

        private MailboxExportService ExportService
        {
            get
            {
                return new MailboxExportService(new XstFileExportOperations(mailboxSessionService.CurrentFile));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = view;

            // For testing purposes, use these flags to control the display of print headers
            //view.DisplayPrintHeaders = true;
            //view.DisplayEmailType = true;

            // Supply the Search control with the list of sections
            searchTextBox.SectionsList = new List<string> { "Subject", "From/To", "Date", "Cc", "Bcc" };
            searchTextBox.SectionsInitiallySelected = new List<bool> { true, true, true, false, false };

            if (settings.Top != 0.0)
            {
                this.Top = settings.Top;
                this.Left = settings.Left;
                this.Height = settings.Height;
                this.Width = settings.Width;
            }
        }

        public void OpenFile(string fileName)
        {
            if (!System.IO.File.Exists(fileName))
                return;

            view.Clear();
            ShowStatus("Loading...");
            Mouse.OverrideCursor = Cursors.Wait;

            // Load on a background thread so we can keep the UI in sync
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var root = mailboxSessionService.OpenMailbox(fileName);

                    //
                    // I refactorized this part: 
                    //
                    // iterator with view.RootFolderViews.Add -> moved to view.UpdateFolderViews(root)
                    //

                    // We may be called on a background thread, so we need to dispatch this to the UI thread
                    Application.Current.Dispatcher.Invoke(new Action(() => view.UpdateFolderViews(root)));

                    //foreach (var f in root.Folders)
                    //{
                    //    // We may be called on a background thread, so we need to dispatch this to the UI thread
                    //    Application.Current.Dispatcher.Invoke(new Action(() =>
                    //    {
                    //        view.RootFolderViews.Add(new FolderView(f));
                    //    }));
                    //}
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error reading xst file");
                }
            })
            // When loading completes, update the UI using the UI thread 
            .ContinueWith((task) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ShowStatus(null);
                    Mouse.OverrideCursor = null;
                    Title = "Xst Reader - " + System.IO.Path.GetFileName(fileName);
                }));
            });
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            // Ask for a .ost or .pst file to open
            string fileName = GetXstFileName();

            if (fileName != null)
                OpenFile(fileName);
        }

        private void exportAllProperties_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ExportEmailProperties(view.SelectedFolder.MessageViews);
        }

        private void exportAllEmails_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ExportEmails(view.SelectedFolder.MessageViews);
        }

        private void treeFolders_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                FolderView fv = (FolderView)e.NewValue;
                view.SelectedFolder = fv;

                if (fv != null)
                {
                    view.SetMessage(null);
                    ShowMessage(null);
                    fv.MessageViews.Clear();
                    ShowStatus("Reading messages...");
                    Mouse.OverrideCursor = Cursors.Wait;

                    // Read messages on a background thread so we can keep the UI in sync
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            mailboxSessionService.LoadFolderMessages(fv.Folder);
                            // We may be called on a background thread, so we need to dispatch this to the UI thread
                            Application.Current.Dispatcher.Invoke(new Action(() => fv.UpdateMessageViews()));
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), "Error reading messages");
                        }
                    })
                    // When loading completes, update the UI using the UI thread 
                    .ContinueWith((task) =>
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            ShowStatus(null);
                            Mouse.OverrideCursor = null;
                        }));
                    });

                    // If there is no sort in effect, sort by date in descending order
                    SortMessages("Date", ListSortDirection.Descending, ifNoneAlready: true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Unexpected error reading messages");
            }
        }

        private void listMessages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            searchIndex = listMessages.SelectedIndex;
            searchTextBox.ShowSearch = true;
            MessageView mv = (MessageView)listMessages.SelectedItem;

            if (mv != null)
            {
                try
                {
                    mailboxSessionService.LoadMessageDetails(mv.Message);
                    ShowMessage(mv);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error reading message details");
                }
            }
            view.SetMessage(mv);
        }

        private void listMessagesColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            // Sort the messages by the clicked on column
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            SortMessages(column.Tag.ToString(), ListSortDirection.Ascending);

            searchIndex = listMessages.SelectedIndex;
            listMessages.ScrollIntoView(listMessages.SelectedItem);
        }

        private void listRecipients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                view.SelectedRecipientChanged((Recipient)listRecipients.SelectedItem);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error reading recipient");
            }
        }

        private void listAttachments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                view.SelectedAttachmentsChanged(listAttachments.SelectedItems.Cast<Attachment>());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error reading attachment");
            }
        }

        private void exportEmail_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (listMessages.SelectedItems.Count > 1)
            {
                ExportEmails(listMessages.SelectedItems.Cast<MessageView>());
            }
            else
            {
                string fullFileName = GetEmailExportFileName(view.CurrentMessage.ExportFileName,
                                            view.CurrentMessage.ExportFileExtension);

                if (fullFileName != null)
                {
                    try
                    {
                        ExportService.ExportEmail(view.CurrentMessage.Message, fullFileName);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error exporting email");
                    }
                }
            }
        }

        private void SortMessages(string property, ListSortDirection direction, bool ifNoneAlready = false)
        {
            var targetCol = ((GridView)listMessages.View).Columns.Select(c => (GridViewColumnHeader)c.Header)
                            .First(h => h.Tag.ToString() == property);

            var sorts = listMessages.Items.SortDescriptions;
            MessageSortPlan sortPlan = MessageSortService.BuildSortPlan(property, direction, sorts.Cast<SortDescription>(), ifNoneAlready);
            if (!sortPlan.ShouldApply)
                return;

            var matches = sorts.Where(s => s.PropertyName == property);
            if (matches.Count() > 0)
            {
                sorts.Remove(matches.First());
            }
            //else
            //{
            //    // If there is not one, see if we have the maximum number of sorts applied already
            //    // The algorithm works for any limit with no changes other than this test
            //    if (sorts.Count >= 2)
            //    {
            //        // If so, remove the oldest one
            //        var oldSort = sorts.Last();
            //        var oldCol = ((GridView)listMessages.View).Columns.Select(c => (GridViewColumnHeader)c.Header)
            //               .First(h => h.Tag.ToString() == oldSort.PropertyName);
            //        sorts.Remove(oldSort);

            //        // And the adorner that went with it
            //        var oldAdorners = AdornerLayer.GetAdornerLayer(oldCol);
            //        var oldAdorner = oldAdorners.GetAdorners(oldCol)?.Cast<SortAdorner>()?.FirstOrDefault(s => s != null);
            //        if (oldAdorner != null)
            //            oldAdorners.Remove(oldAdorner);
            //    }
            //}

            // Apply the requested sort as the dominant one, whatever it was before
            sorts.Insert(0, new SortDescription(property, sortPlan.Direction));

            // Find any sort adorner applied to the target column
            var adorners = AdornerLayer.GetAdornerLayer(targetCol);
            var adorner = adorners.GetAdorners(targetCol)?.Cast<SortAdorner>()?.FirstOrDefault(s => s != null);
            // If there is one, remove it
            if (adorner != null)
                adorners.Remove(adorner);

            // Create and apply the requested adorner
            adorner = new SortAdorner(targetCol, sortPlan.Direction);
            adorners.Add(adorner);
        }

        private void ExportEmails(IEnumerable<MessageView> messages)
        {
            string folderName = GetEmailsExportFolderName();

            if (folderName != null)
            {
                ShowStatus("Exporting emails...");
                Mouse.OverrideCursor = Cursors.Wait;

                // Export emails on a background thread so we can keep the UI in sync
                Task.Factory.StartNew<Tuple<int, int>>(() =>
                {
                    var result = ExportService.ExportEmails(
                        messages.Select(mv => mv.Message),
                        folderName,
                        reportProgress: status => Application.Current.Dispatcher.Invoke(new Action(() => ShowStatus(status))),
                        shouldContinue: failure =>
                        {
                            var choice = Application.Current.Dispatcher.Invoke(() =>
                                MessageBox.Show(
                                    String.Format("Error '{0}' exporting email '{1}'",
                                        failure.Exception.Message, failure.Message.Subject),
                                    "Error exporting emails",
                                    MessageBoxButton.OKCancel));
                            return choice != MessageBoxResult.Cancel;
                        });

                    return new Tuple<int, int>(result.SuccessfulCount, result.FailedCount);
                })
                // When exporting completes, update the UI using the UI thread 
                .ContinueWith((task) =>
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ShowStatus(null);
                        Mouse.OverrideCursor = null;
                        txtStatus.Text = String.Format("Completed with {0} successes and {1} failures",
                            task.Result.Item1, task.Result.Item2);
                    }));
                });
            }
        }

        private void ExportEmailProperties(IEnumerable<MessageView> messages)
        {
            string fileName = GetPropertiesExportFileName(view.SelectedFolder.Name);

            if (fileName != null)
            {
                ShowStatus("Exporting properties...");
                Mouse.OverrideCursor = Cursors.Wait;

                // Export properties on a background thread so we can keep the UI in sync
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ExportService.ExportMessageProperties(messages.Select(v => v.Message), fileName);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error exporting properties");
                    }
                })
                // When exporting completes, update the UI using the UI thread 
                .ContinueWith((task) =>
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ShowStatus(null);
                        Mouse.OverrideCursor = null;
                    }));
                });
            }
        }

        private void SaveAttachments(IEnumerable<Attachment> attachments)
        {
            string folderName = GetAttachmentsSaveFolderName();

            if (folderName != null)
            {
                try
                {
                    ExportService.SaveAttachmentsToFolder(folderName, view.CurrentMessage.Date, attachments);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(String.Format("Error '{0}' saving attachments to '{1}'",
                        ex.Message, view.CurrentMessage.Subject), "Error saving attachments");
                }
            }
        }

        private void exportEmailProperties_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (listMessages.SelectedItems.Count > 1)
            {
                ExportEmailProperties(listMessages.SelectedItems.Cast<MessageView>());
            }
            else
            {
                string fileName = GetPropertiesExportFileName(view.CurrentMessage.ExportFileName);

                if (fileName != null)
                    ExportService.ExportMessageProperties(new Message[1] { view.CurrentMessage.Message }, fileName);
            }
        }

        private void btnSaveAllAttachments_Click(object sender, RoutedEventArgs e)
        {
            SaveAttachments(view.CurrentMessage.Attachments);
        }

        private void btnCloseEmail_Click(object sender, RoutedEventArgs e)
        {
            view.PopMessage();
            ShowMessage(view.CurrentMessage);
        }

        private void rbContent_Click(object sender, RoutedEventArgs e)
        {
            view.ShowContent = true;
        }

        private void rbProperties_Click(object sender, RoutedEventArgs e)
        {
            view.ShowContent = false;
        }

        private void searchTextBox_OnSearch(object sender, RoutedEventArgs e)
        {
            try
            {
                var args = e as SearchEventArgs;
                MessageSearchSections sections = MessageSearchService.ParseSections(args.Sections);
                List<MessageView> messages = listMessages.Items.Cast<MessageView>().ToList();
                MessageSearchResult result = MessageSearchService.FindMatch(
                    messages,
                    args.Keyword,
                    sections,
                    args.SearchEventType,
                    searchIndex);

                if (result.Found)
                {
                    ApplySearchSelection(messages[result.Index], result.Index);
                    if (result.ShouldShowSearch)
                        searchTextBox.ShowSearch = true;
                }
                else
                {
                    if (args.SearchEventType == SearchEventType.Search)
                        searchIndex = -1;
                    searchTextBox.IndicateSearchFailed(args.SearchEventType);
                }
            }
            catch 
            {
                // Unclear what we can do here, as we were invoked by an event from the search text box control
            }
        }

        private void ApplySearchSelection(MessageView messageView, int index)
        {
            searchIndex = index;
            listMessages.UnselectAll();
            messageView.IsSelected = true;
            listMessages.ScrollIntoView(messageView);
        }

        private void ShowStatus(string status)
        {
            if (status != null)
            {
                view.IsBusy = true;
                txtStatus.Text = status;
            }
            else
            {
                view.IsBusy = false;
                txtStatus.Text = "";
            }
        }

        private void ShowMessage(MessageView mv)
        {
            try
            {
                //clear any existing status
                ShowStatus(null);

                if (mv != null)
                {
                    //email is signed and/or encrypted and no body was included
                    if (mv.IsEncryptedOrSigned)
                    {
                        try
                        {
                            mv.ReadSignedOrEncryptedMessage(mailboxSessionService.CurrentFile);
                        }
                        catch
                        {
                            ShowStatus("Message Failed to Decrypt");
                        }
                    }

                    // Populate the view of the attachments
                    mv.SortAndSaveAttachments(mv.Message.Attachments);

                    // Can't bind HTML content, so push it into the control, if the message is HTML
                    if (mv.ShowHtml)
                    {
                        string body = mv.Message.GetBodyAsHtmlString();
                        if (mv.MayHaveInlineAttachment)
                            body = mv.Message.EmbedAttachments(body, mailboxSessionService.CurrentFile);  // Returns null if this is not appropriate

                        if (body != null)
                        {
                            // For testing purposes, can show print header in main visualisation
                            if (view.DisplayPrintHeaders)
                                body = mv.Message.EmbedHtmlPrintHeader(body, view.DisplayEmailType);

                            wbMessage.NavigateToString(body);
                            if (mv.MayHaveInlineAttachment)
                            {
                                mv.SortAndSaveAttachments();  // Re-sort attachments in case any new in-line rendering discovered
                            }
                        }
                    }
                    // Can't bind RTF content, so push it into the control, if the message is RTF
                    else if (mv.ShowRtf)
                    {
                        var body = mv.Message.GetBodyAsFlowDocument();

                        // For testing purposes, can show print header in main visualisation
                        if (view.DisplayPrintHeaders)
                            mv.Message.EmbedRtfPrintHeader(body, view.DisplayEmailType);

                        rtfMessage.Document = body;
                    }
                    // Could bind text content, but use push so that we can optionally add headers
                    else if (mv.ShowText)
                    {
                        var body = mv.Body;

                        // For testing purposes, can show print header in main visualisation
                        if (view.DisplayPrintHeaders)
                            body = mv.Message.EmbedTextPrintHeader(body, true, view.DisplayEmailType);

                        txtMessage.Text = body;
                        scrollTextMessage.ScrollToHome();
                    }
                }
                else
                {
                    // Clear the displays, in case we were showing that type before
                    wbMessage.Navigate("about:blank");
                    rtfMessage.Document.Blocks.Clear();
                    txtMessage.Text = "";
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error reading message body");
            }
        }

        private void OpenEmailAttachment(Attachment a)
        {
            Message m = mailboxSessionService.CurrentFile.OpenAttachedMessage(a);
            var mv = new MessageView(m);
            ShowMessage(mv);
            view.PushMessage(mv);
        }

        #region File and folder dialogs

        private string GetXstFileName()
        {
            // Ask for a .ost or .pst file to open
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();

            dialog.Filter = "xst files (*.ost;*.pst)|*.ost;*.pst|All files (*.*)|*.*";
            dialog.FilterIndex = 1;
            dialog.InitialDirectory = settings.LastFolder;
            if (dialog.InitialDirectory == "")
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                settings.LastFolder = Path.GetDirectoryName(dialog.FileName);
                settings.Save();
                return dialog.FileName;
            }
            else
                return null;
        }

        private string GetAttachmentsSaveFolderName()
        {
            // Find out where to save the attachments
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            dialog.Description = "Choose folder for saving attachments";
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            dialog.SelectedPath = settings.LastAttachmentFolder;
            if (dialog.SelectedPath == "")
                dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                settings.LastAttachmentFolder = dialog.SelectedPath;
                settings.Save();
                return dialog.SelectedPath;
            }
            else
                return null;
        }

        private string GetSaveAttachmentFileName(string defaultFileName)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog();

            dialog.Title = "Specify file to save to";
            dialog.InitialDirectory = settings.LastAttachmentFolder;
            if (dialog.InitialDirectory == "")
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.Filter = "All Files (*.*)|*.*";
            dialog.FileName = defaultFileName;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                settings.LastAttachmentFolder = Path.GetDirectoryName(dialog.FileName);
                settings.Save();
                return dialog.FileName;
            }
            else
                return null;
        }

        private string GetEmailsExportFolderName()
        {
            // Find out where to export the emails
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            dialog.Description = "Choose folder to export emails into";
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            dialog.SelectedPath = settings.LastExportFolder;
            if (dialog.SelectedPath == "")
                dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                settings.LastExportFolder = dialog.SelectedPath;
                settings.Save();
                return dialog.SelectedPath;
            }
            else
                return null;
        }

        private string GetEmailExportFileName(string defaultFileName, string extension)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog();

            dialog.Title = "Specify file to save to";
            dialog.InitialDirectory = settings.LastExportFolder;
            if (dialog.InitialDirectory == "")
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.Filter = String.Format("{0} Files (*.{0})|*.{0}|All Files (*.*)|*.*", extension);
            dialog.FileName = defaultFileName;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                settings.LastExportFolder = Path.GetDirectoryName(dialog.FileName);
                settings.Save();
                return dialog.FileName;
            }
            else
                return null;
        }

        private string GetPropertiesExportFileName(string defaultName)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog();

            dialog.Title = "Specify properties export file";
            dialog.InitialDirectory = settings.LastExportFolder;
            if (dialog.InitialDirectory == "")
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.Filter = "csv files (*.csv)|*.csv";
            dialog.FileName = defaultName;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                settings.LastExportFolder = Path.GetDirectoryName(dialog.FileName);
                settings.Save();
                return dialog.FileName;
            }
            else
                return null;
        }
        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Clean up temporary files
            foreach (var fileFullName in tempFileNames)
            {
                // Wrap in try in case the file is still open
                try
                {
                    File.Delete(fileFullName);
                }
                catch { }
            }

            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                settings.Top = RestoreBounds.Top;
                settings.Left = RestoreBounds.Left;
                settings.Height = RestoreBounds.Height;
                settings.Width = RestoreBounds.Width;
            }
            else
            {
                settings.Top = this.Top;
                settings.Left = this.Left;
                settings.Height = this.Height;
                settings.Width = this.Width;
            }
            settings.Save();
        }

        private string SaveAttachmentToTemporaryFile(Attachment a)
        {
            if (a == null)
                return null;

            string fileFullName = Path.ChangeExtension(
                Path.GetTempPath() + Guid.NewGuid().ToString(), Path.GetExtension(a.FileName)); ;

            try
            {
                ExportService.SaveAttachment(fileFullName, null, a);
                tempFileNames.Add(fileFullName);
                return fileFullName;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error saving attachment");
                return null;
            }
        }

        private void attachmentEmailCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var a = listAttachments.SelectedItem as Attachment;
            e.CanExecute = a != null && a.IsEmail;
        }

        //private void openEmail_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    var a = listAttachments.SelectedItem as Attachment;
        //    OpenEmailAttachment(a);
        //}

        private void attachmentCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var a = listAttachments.SelectedItem as Attachment;
            e.CanExecute = a != null;
        }

        private void attachmentFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var a = listAttachments.SelectedItem as Attachment;
            e.CanExecute = a != null && a.IsFile;
        }

        private void openAttachment_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var a = listAttachments.SelectedItem as Attachment;

            if (a.IsFile)
            {
                string fileFullname = SaveAttachmentToTemporaryFile(a);
                if (fileFullname == null)
                    return;

                using (Process.Start(fileFullname)) { }
            }
            else if (a.IsEmail)
                OpenEmailAttachment(a);

            e.Handled = true;
        }

        private void openAttachmentWith_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var a = listAttachments.SelectedItem as Attachment;
            string fileFullname = SaveAttachmentToTemporaryFile(a);
            if (fileFullname == null)
                return;

            if (Environment.OSVersion.Version.Major > 5)
            {
                IntPtr hwndParent = Process.GetCurrentProcess().MainWindowHandle;
                tagOPENASINFO oOAI = new tagOPENASINFO();
                oOAI.cszFile = fileFullname;
                oOAI.cszClass = String.Empty;
                oOAI.oaifInFlags = tagOPEN_AS_INFO_FLAGS.OAIF_ALLOW_REGISTRATION | tagOPEN_AS_INFO_FLAGS.OAIF_EXEC;
                SHOpenWithDialog(hwndParent, ref oOAI);
            }
            else
            {
                using (Process.Start("rundll32", "shell32.dll,OpenAs_RunDLL " + fileFullname)) { }
            }
            e.Handled = true;
        }

        private void saveAttachmentAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (listAttachments.SelectedItems.Count > 1)
            {
                SaveAttachments(listAttachments.SelectedItems.Cast<Attachment>());
            }
            else
            {
                var a = listAttachments.SelectedItem as Attachment;
                var fullFileName = GetSaveAttachmentFileName(a.LongFileName);

                if (fullFileName != null)
                {
                    try
                    {
                        ExportService.SaveAttachment(fullFileName, view.CurrentMessage.Date, a);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error saving attachment");
                    }
                }
            }
            e.Handled = true;
        }

        // Plumbing to enable access to SHOpenWithDialog
        [DllImport("shell32.dll", EntryPoint = "SHOpenWithDialog", CharSet = CharSet.Unicode)]
        private static extern int SHOpenWithDialog(IntPtr hWndParent, ref tagOPENASINFO oOAI);
        private struct tagOPENASINFO
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string cszFile;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string cszClass;

            [MarshalAs(UnmanagedType.I4)]
            public tagOPEN_AS_INFO_FLAGS oaifInFlags;
        }
        [Flags]
        private enum tagOPEN_AS_INFO_FLAGS
        {
            OAIF_ALLOW_REGISTRATION = 0x00000001,   // Show "Always" checkbox
            OAIF_REGISTER_EXT = 0x00000002,   // Perform registration when user hits OK
            OAIF_EXEC = 0x00000004,   // Exec file after registering
            OAIF_FORCE_REGISTRATION = 0x00000008,   // Force the checkbox to be registration
            OAIF_HIDE_REGISTRATION = 0x00000020,   // Vista+: Hide the "always use this file" checkbox
            OAIF_URL_PROTOCOL = 0x00000040,   // Vista+: cszFile is actually a URI scheme; show handlers for that scheme
            OAIF_FILE_IS_URI = 0x00000080    // Win8+: The location pointed to by the pcszFile parameter is given as a URI
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string Repository = "https://github.com/NeedsCoffee/XstReaderNext";

            StringBuilder msg = new StringBuilder(100);
            msg.AppendLine("View Microsoft Outlook Mail files");
            msg.Append("Version: ");
            msg.AppendLine(version.ToString());
            msg.Append("Repository: ");
            msg.AppendLine(Repository);

            MessageBox.Show(msg.ToString(), "About XstReader");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XstReader
{
    internal interface IMailboxExportOperations
    {
        void ReadMessageDetails(Message message);
        void ExportMessageToFile(Message message, string fullFileName);
        void SaveVisibleAttachmentsToAssociatedFolder(string fullFileName, Message message);
        void ExportMessageProperties(IEnumerable<Message> messages, string fileName);
        void SaveAttachmentsToFolder(string fullFolderName, DateTime? creationTime, IEnumerable<Attachment> attachments);
        void SaveAttachment(string fullFileName, DateTime? creationTime, Attachment attachment);
    }

    internal sealed class XstFileExportOperations : IMailboxExportOperations
    {
        private readonly XstFile xstFile;

        public XstFileExportOperations(XstFile xstFile)
        {
            this.xstFile = xstFile ?? throw new ArgumentNullException(nameof(xstFile));
        }

        public void ReadMessageDetails(Message message) => xstFile.ReadMessageDetails(message);

        public void ExportMessageToFile(Message message, string fullFileName) =>
            message.ExportToFile(fullFileName, xstFile);

        public void SaveVisibleAttachmentsToAssociatedFolder(string fullFileName, Message message) =>
            xstFile.SaveVisibleAttachmentsToAssociatedFolder(fullFileName, message);

        public void ExportMessageProperties(IEnumerable<Message> messages, string fileName) =>
            xstFile.ExportMessageProperties(messages, fileName);

        public void SaveAttachmentsToFolder(string fullFolderName, DateTime? creationTime, IEnumerable<Attachment> attachments) =>
            xstFile.SaveAttachmentsToFolder(fullFolderName, creationTime, attachments);

        public void SaveAttachment(string fullFileName, DateTime? creationTime, Attachment attachment) =>
            xstFile.SaveAttachment(fullFileName, creationTime, attachment);
    }

    internal sealed class MailboxExportService
    {
        private readonly IMailboxExportOperations operations;

        public MailboxExportService(IMailboxExportOperations operations)
        {
            this.operations = operations ?? throw new ArgumentNullException(nameof(operations));
        }

        public EmailExportResult ExportEmails(
            IEnumerable<Message> messages,
            string folderName,
            Action<string> reportProgress = null,
            Func<EmailExportFailure, bool> shouldContinue = null)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));
            if (folderName == null)
                throw new ArgumentNullException(nameof(folderName));

            int good = 0;
            int bad = 0;
            bool cancelled = false;
            HashSet<string> usedNames = new HashSet<string>();

            foreach (Message message in messages)
            {
                try
                {
                    string exportBaseName = GetUniqueExportFileName(message.ExportFileName, usedNames);
                    reportProgress?.Invoke("Exporting " + message.ExportFileName);

                    operations.ReadMessageDetails(message);
                    string fullFileName = Path.Combine(folderName, $"{exportBaseName}.{message.ExportFileExtension}");
                    operations.ExportMessageToFile(message, fullFileName);
                    operations.SaveVisibleAttachmentsToAssociatedFolder(fullFileName, message);
                    good++;
                }
                catch (Exception ex)
                {
                    bad++;
                    if (shouldContinue != null && !shouldContinue(new EmailExportFailure(message, ex)))
                    {
                        cancelled = true;
                        break;
                    }
                }
            }

            return new EmailExportResult(good, bad, cancelled);
        }

        public void ExportEmail(Message message, string fullFileName)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (fullFileName == null)
                throw new ArgumentNullException(nameof(fullFileName));

            operations.ReadMessageDetails(message);
            operations.ExportMessageToFile(message, fullFileName);
            operations.SaveVisibleAttachmentsToAssociatedFolder(fullFileName, message);
        }

        public void ExportMessageProperties(IEnumerable<Message> messages, string fileName)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            operations.ExportMessageProperties(messages, fileName);
        }

        public void SaveAttachmentsToFolder(string folderName, DateTime? creationTime, IEnumerable<Attachment> attachments)
        {
            if (folderName == null)
                throw new ArgumentNullException(nameof(folderName));
            if (attachments == null)
                throw new ArgumentNullException(nameof(attachments));

            operations.SaveAttachmentsToFolder(folderName, creationTime, attachments);
        }

        public void SaveAttachment(string fullFileName, DateTime? creationTime, Attachment attachment)
        {
            if (fullFileName == null)
                throw new ArgumentNullException(nameof(fullFileName));
            if (attachment == null)
                throw new ArgumentNullException(nameof(attachment));

            operations.SaveAttachment(fullFileName, creationTime, attachment);
        }

        internal static string GetUniqueExportFileName(string exportFileName, ISet<string> usedNames)
        {
            if (exportFileName == null)
                throw new ArgumentNullException(nameof(exportFileName));
            if (usedNames == null)
                throw new ArgumentNullException(nameof(usedNames));

            string fileName = exportFileName;
            for (int i = 1; ; i++)
            {
                if (!usedNames.Contains(fileName))
                {
                    usedNames.Add(fileName);
                    return fileName;
                }

                fileName = $"{exportFileName} ({i})";
            }
        }
    }

    internal sealed record EmailExportFailure(Message Message, Exception Exception);

    internal sealed record EmailExportResult(int SuccessfulCount, int FailedCount, bool Cancelled);
}

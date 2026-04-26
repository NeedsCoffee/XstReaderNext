using System;

namespace XstReader
{
    internal interface IMailboxSessionOperations
    {
        XstFile OpenMailbox(string fileName);
        Folder ReadFolderTree(XstFile xstFile);
        void ReadMessages(XstFile xstFile, Folder folder);
        void ReadMessageDetails(XstFile xstFile, Message message);
    }

    internal sealed class XstFileMailboxSessionOperations : IMailboxSessionOperations
    {
        public XstFile OpenMailbox(string fileName) => new XstFile(fileName);

        public Folder ReadFolderTree(XstFile xstFile) => xstFile.ReadFolderTree();

        public void ReadMessages(XstFile xstFile, Folder folder) => xstFile.ReadMessages(folder);

        public void ReadMessageDetails(XstFile xstFile, Message message) => xstFile.ReadMessageDetails(message);
    }

    internal sealed class MailboxSessionService
    {
        private readonly IMailboxSessionOperations operations;
        private XstFile currentFile;

        public MailboxSessionService(IMailboxSessionOperations operations)
        {
            this.operations = operations ?? throw new ArgumentNullException(nameof(operations));
        }

        public XstFile CurrentFile
        {
            get
            {
                if (currentFile == null)
                    throw new InvalidOperationException("No mailbox is currently loaded.");

                return currentFile;
            }
        }

        public Folder OpenMailbox(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            currentFile = operations.OpenMailbox(fileName);
            return operations.ReadFolderTree(CurrentFile);
        }

        public void LoadFolderMessages(Folder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            operations.ReadMessages(CurrentFile, folder);
        }

        public void LoadMessageDetails(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            operations.ReadMessageDetails(CurrentFile, message);
        }
    }
}

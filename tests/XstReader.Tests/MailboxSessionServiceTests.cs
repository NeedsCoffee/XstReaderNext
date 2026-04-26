using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace XstReader.Tests;

public class MailboxSessionServiceTests
{
    [Fact]
    public void CurrentFile_ThrowsBeforeMailboxIsOpened()
    {
        var service = new MailboxSessionService(new FakeMailboxSessionOperations());

        Assert.Throws<InvalidOperationException>(() => _ = service.CurrentFile);
    }

    [Fact]
    public void OpenMailbox_StoresCurrentFileAndReturnsRootFolder()
    {
        var operations = new FakeMailboxSessionOperations();
        var service = new MailboxSessionService(operations);

        Folder root = service.OpenMailbox("sample.pst");

        Assert.Same(operations.OpenedFile, service.CurrentFile);
        Assert.Same(operations.RootFolder, root);
        Assert.Equal("sample.pst", operations.OpenedFileName);
        Assert.Same(operations.OpenedFile, operations.ReadFolderTreeFile);
    }

    [Fact]
    public void LoadFolderMessages_UsesCurrentMailboxAndProvidedFolder()
    {
        var operations = new FakeMailboxSessionOperations();
        var service = new MailboxSessionService(operations);
        Folder folder = new() { Name = "Inbox" };
        service.OpenMailbox("sample.pst");

        service.LoadFolderMessages(folder);

        Assert.Same(operations.OpenedFile, operations.ReadMessagesFile);
        Assert.Same(folder, operations.ReadMessagesFolder);
    }

    [Fact]
    public void LoadMessageDetails_UsesCurrentMailboxAndProvidedMessage()
    {
        var operations = new FakeMailboxSessionOperations();
        var service = new MailboxSessionService(operations);
        Message message = new() { Subject = "Hello", Folder = new Folder { Name = "Inbox" } };
        service.OpenMailbox("sample.pst");

        service.LoadMessageDetails(message);

        Assert.Same(operations.OpenedFile, operations.ReadMessageDetailsFile);
        Assert.Same(message, operations.ReadMessageDetailsMessage);
    }

    private sealed class FakeMailboxSessionOperations : IMailboxSessionOperations
    {
        public XstFile OpenedFile { get; } = (XstFile)RuntimeHelpers.GetUninitializedObject(typeof(XstFile));
        public Folder RootFolder { get; } = new() { Name = "Root" };
        public string? OpenedFileName { get; private set; }
        public XstFile? ReadFolderTreeFile { get; private set; }
        public XstFile? ReadMessagesFile { get; private set; }
        public Folder? ReadMessagesFolder { get; private set; }
        public XstFile? ReadMessageDetailsFile { get; private set; }
        public Message? ReadMessageDetailsMessage { get; private set; }

        public XstFile OpenMailbox(string fileName)
        {
            OpenedFileName = fileName;
            return OpenedFile;
        }

        public Folder ReadFolderTree(XstFile xstFile)
        {
            ReadFolderTreeFile = xstFile;
            return RootFolder;
        }

        public void ReadMessages(XstFile xstFile, Folder folder)
        {
            ReadMessagesFile = xstFile;
            ReadMessagesFolder = folder;
        }

        public void ReadMessageDetails(XstFile xstFile, Message message)
        {
            ReadMessageDetailsFile = xstFile;
            ReadMessageDetailsMessage = message;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XstReader.Tests;

public class MailboxExportServiceTests
{
    [Fact]
    public void GetUniqueExportFileName_AppendsNumericSuffixesForDuplicates()
    {
        var usedNames = new HashSet<string>();

        Assert.Equal("Weekly Update", MailboxExportService.GetUniqueExportFileName("Weekly Update", usedNames));
        Assert.Equal("Weekly Update (1)", MailboxExportService.GetUniqueExportFileName("Weekly Update", usedNames));
        Assert.Equal("Weekly Update (2)", MailboxExportService.GetUniqueExportFileName("Weekly Update", usedNames));
    }

    [Fact]
    public void ExportEmails_ContinuesAfterFailure_WhenHandlerAllowsIt()
    {
        var operations = new FakeMailboxExportOperations();
        var service = new MailboxExportService(operations);
        Message first = CreateMessage("First");
        Message second = CreateMessage("Second");
        operations.Failures.Add(second);

        EmailExportResult result = service.ExportEmails(
            new[] { first, second, CreateMessage("Third") },
            Path.GetTempPath(),
            shouldContinue: _ => true);

        Assert.Equal(2, result.SuccessfulCount);
        Assert.Equal(1, result.FailedCount);
        Assert.False(result.Cancelled);
        Assert.Equal(3, operations.ReadMessageDetailsCalls.Count);
        Assert.Equal(2, operations.SaveVisibleAttachmentsCalls.Count);
    }

    [Fact]
    public void ExportEmails_StopsAfterFailure_WhenHandlerCancels()
    {
        var operations = new FakeMailboxExportOperations();
        var service = new MailboxExportService(operations);
        Message first = CreateMessage("First");
        Message second = CreateMessage("Second");
        Message third = CreateMessage("Third");
        operations.Failures.Add(second);

        EmailExportResult result = service.ExportEmails(
            new[] { first, second, third },
            Path.GetTempPath(),
            shouldContinue: _ => false);

        Assert.Equal(1, result.SuccessfulCount);
        Assert.Equal(1, result.FailedCount);
        Assert.True(result.Cancelled);
        Assert.DoesNotContain(third, operations.ReadMessageDetailsCalls);
    }

    private static Message CreateMessage(string subject)
    {
        return new Message
        {
            Subject = subject,
            Body = "body",
            Received = new DateTime(2024, 1, 1, 9, 30, 0),
            Folder = new Folder { Name = "Inbox" }
        };
    }

    private sealed class FakeMailboxExportOperations : IMailboxExportOperations
    {
        public List<Message> ReadMessageDetailsCalls { get; } = new();
        public List<(string Path, Message Message)> SaveVisibleAttachmentsCalls { get; } = new();
        public HashSet<Message> Failures { get; } = new();

        public void ReadMessageDetails(Message message)
        {
            ReadMessageDetailsCalls.Add(message);

            if (Failures.Contains(message))
                throw new InvalidOperationException("boom");
        }

        public void SaveVisibleAttachmentsToAssociatedFolder(string fullFileName, Message message)
        {
            SaveVisibleAttachmentsCalls.Add((fullFileName, message));
        }

        public void ExportMessageToFile(Message message, string fullFileName)
        {
        }

        public void ExportMessageProperties(IEnumerable<Message> messages, string fileName)
        {
        }

        public void SaveAttachmentsToFolder(string fullFolderName, DateTime? creationTime, IEnumerable<Attachment> attachments)
        {
        }

        public void SaveAttachment(string fullFileName, DateTime? creationTime, Attachment attachment)
        {
        }
    }
}

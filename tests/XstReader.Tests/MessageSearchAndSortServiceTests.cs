using SearchTextBox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace XstReader.Tests;

public class MessageSearchAndSortServiceTests
{
    [Fact]
    public void ParseSections_CombinesRecognizedValues()
    {
        MessageSearchSections sections = MessageSearchService.ParseSections(new[] { "Subject", "Cc", "Ignored" });

        Assert.True(sections.HasFlag(MessageSearchSections.Subject));
        Assert.True(sections.HasFlag(MessageSearchSections.Cc));
        Assert.False(sections.HasFlag(MessageSearchSections.FromTo));
    }

    [Fact]
    public void FindMatch_SearchStartsFromTop()
    {
        List<MessageView> messages = CreateMessages();

        MessageSearchResult result = MessageSearchService.FindMatch(
            messages,
            "beta",
            MessageSearchSections.Subject,
            SearchEventType.Search,
            currentIndex: 2);

        Assert.True(result.Found);
        Assert.Equal(1, result.Index);
        Assert.False(result.ShouldShowSearch);
    }

    [Fact]
    public void FindMatch_NextStartsAfterCurrentIndex_AndRequestsSearchReset()
    {
        List<MessageView> messages = CreateMessages();

        MessageSearchResult result = MessageSearchService.FindMatch(
            messages,
            "team",
            MessageSearchSections.FromTo,
            SearchEventType.Next,
            currentIndex: 0);

        Assert.True(result.Found);
        Assert.Equal(2, result.Index);
        Assert.True(result.ShouldShowSearch);
    }

    [Fact]
    public void FindMatch_PreviousSearchesBackwards()
    {
        List<MessageView> messages = CreateMessages();

        MessageSearchResult result = MessageSearchService.FindMatch(
            messages,
            "copy",
            MessageSearchSections.Cc,
            SearchEventType.Previous,
            currentIndex: 2);

        Assert.True(result.Found);
        Assert.Equal(1, result.Index);
    }

    [Fact]
    public void FindMatch_ReturnsNotFoundWhenNoMessageMatches()
    {
        List<MessageView> messages = CreateMessages();

        MessageSearchResult result = MessageSearchService.FindMatch(
            messages,
            "missing",
            MessageSearchSections.Subject | MessageSearchSections.FromTo,
            SearchEventType.Search,
            currentIndex: -1);

        Assert.False(result.Found);
        Assert.Equal(-1, result.Index);
    }

    [Fact]
    public void BuildSortPlan_TogglesDirectionWhenPropertyAlreadySorted()
    {
        MessageSortPlan plan = MessageSortService.BuildSortPlan(
            "Date",
            ListSortDirection.Ascending,
            new[] { new SortDescription("Date", ListSortDirection.Ascending) });

        Assert.True(plan.ShouldApply);
        Assert.Equal(ListSortDirection.Descending, plan.Direction);
    }

    [Fact]
    public void BuildSortPlan_SkipsDefaultSortWhenAnySortAlreadyExists()
    {
        MessageSortPlan plan = MessageSortService.BuildSortPlan(
            "Date",
            ListSortDirection.Descending,
            new[] { new SortDescription("Subject", ListSortDirection.Ascending) },
            ifNoneAlready: true);

        Assert.False(plan.ShouldApply);
        Assert.Equal(ListSortDirection.Descending, plan.Direction);
    }

    private static List<MessageView> CreateMessages()
    {
        return new List<MessageView>
        {
            CreateMessageView("Alpha", "Alice", "2024-01-01 09:00", "", ""),
            CreateMessageView("Beta release", "Bob", "2024-01-02 09:00", "Copy Team", ""),
            CreateMessageView("Gamma", "Team Lead", "2024-01-03 09:00", "", "Audit")
        };
    }

    private static MessageView CreateMessageView(string subject, string from, string receivedText, string cc, string bcc)
    {
        Message message = new()
        {
            Subject = subject,
            From = from,
            Received = DateTime.Parse(receivedText),
            Folder = new Folder { Name = "Inbox" },
            Cc = cc
        };

        if (!string.IsNullOrEmpty(cc))
            message.Recipients.Add(new Recipient { DisplayName = cc, RecipientType = RecipientType.Cc });

        if (!string.IsNullOrEmpty(bcc))
            message.Recipients.Add(new Recipient { DisplayName = bcc, RecipientType = RecipientType.Bcc });

        return new MessageView(message);
    }
}

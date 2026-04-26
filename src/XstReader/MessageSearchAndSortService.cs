using SearchTextBox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace XstReader
{
    [Flags]
    internal enum MessageSearchSections
    {
        None = 0,
        Subject = 1,
        FromTo = 2,
        Date = 4,
        Cc = 8,
        Bcc = 16,
    }

    internal sealed record MessageSearchResult(int Index, bool ShouldShowSearch)
    {
        public bool Found => Index >= 0;
    }

    internal static class MessageSearchService
    {
        public static MessageSearchSections ParseSections(IEnumerable<string> sections)
        {
            MessageSearchSections result = MessageSearchSections.None;

            if (sections == null)
                return result;

            foreach (string section in sections)
            {
                switch (section)
                {
                    case "Subject":
                        result |= MessageSearchSections.Subject;
                        break;
                    case "From/To":
                        result |= MessageSearchSections.FromTo;
                        break;
                    case "Date":
                        result |= MessageSearchSections.Date;
                        break;
                    case "Cc":
                        result |= MessageSearchSections.Cc;
                        break;
                    case "Bcc":
                        result |= MessageSearchSections.Bcc;
                        break;
                }
            }

            return result;
        }

        public static MessageSearchResult FindMatch(
            IReadOnlyList<MessageView> messages,
            string keyword,
            MessageSearchSections sections,
            SearchEventType searchEventType,
            int currentIndex)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            switch (searchEventType)
            {
                case SearchEventType.Search:
                    for (int i = 0; i < messages.Count; i++)
                    {
                        if (IsMatch(messages[i], keyword, sections))
                            return new MessageSearchResult(i, false);
                    }
                    break;
                case SearchEventType.Next:
                    for (int i = currentIndex + 1; i < messages.Count; i++)
                    {
                        if (IsMatch(messages[i], keyword, sections))
                            return new MessageSearchResult(i, true);
                    }
                    break;
                case SearchEventType.Previous:
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (IsMatch(messages[i], keyword, sections))
                            return new MessageSearchResult(i, true);
                    }
                    break;
            }

            return new MessageSearchResult(-1, false);
        }

        public static bool IsMatch(MessageView messageView, string keyword, MessageSearchSections sections)
        {
            if (messageView == null)
                return false;

            return HasMatch(sections, MessageSearchSections.Subject, messageView.Subject, keyword) ||
                   HasMatch(sections, MessageSearchSections.FromTo, messageView.FromTo, keyword) ||
                   HasMatch(sections, MessageSearchSections.Date, messageView.DisplayDate, keyword) ||
                   HasMatch(sections, MessageSearchSections.Cc, messageView.CcDisplayList, keyword) ||
                   HasMatch(sections, MessageSearchSections.Bcc, messageView.BccDisplayList, keyword);
        }

        private static bool HasMatch(MessageSearchSections selectedSections, MessageSearchSections targetSection, string value, string keyword)
        {
            return selectedSections.HasFlag(targetSection) &&
                   value != null &&
                   value.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
    }

    internal sealed record MessageSortPlan(bool ShouldApply, ListSortDirection Direction);

    internal static class MessageSortService
    {
        public static MessageSortPlan BuildSortPlan(
            string property,
            ListSortDirection requestedDirection,
            IEnumerable<SortDescription> existingSorts,
            bool ifNoneAlready = false)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            List<SortDescription> sorts = existingSorts?.ToList() ?? new List<SortDescription>();
            if (ifNoneAlready && sorts.Count > 0)
                return new MessageSortPlan(false, requestedDirection);

            SortDescription? existingSort = sorts.FirstOrDefault(sort => sort.PropertyName == property);
            if (existingSort.HasValue)
            {
                requestedDirection = existingSort.Value.Direction == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }

            return new MessageSortPlan(true, requestedDirection);
        }
    }
}

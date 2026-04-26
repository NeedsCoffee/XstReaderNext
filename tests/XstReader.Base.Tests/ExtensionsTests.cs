using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace XstReader.Base.Tests;

public class ExtensionsTests
{
    [Fact]
    public void Truncate_ReturnsOriginalString_WhenShortEnough()
    {
        Assert.Equal("pst", "pst".Truncate(8));
    }

    [Fact]
    public void Truncate_TrimsString_WhenLongerThanLimit()
    {
        Assert.Equal("Outlo", "Outlook".Truncate(5));
    }

    [Fact]
    public void ReplaceInvalidFileNameChars_RemovesWindowsInvalidCharacters()
    {
        string cleaned = "inva:lid?name".ReplaceInvalidFileNameChars("_");

        Assert.Equal("inva_lid_name", cleaned);
    }

    [Fact]
    public void PopulateWith_ReplacesCollectionContents()
    {
        var collection = new ObservableCollection<int> { 1, 2, 3 };

        collection.PopulateWith(new List<int> { 5, 8 });

        Assert.Equal(new[] { 5, 8 }, collection);
    }

    [Fact]
    public void Flatten_ReturnsDepthFirstSequenceIncludingRoots()
    {
        var tree = new[]
        {
            new Node("root", new[]
            {
                new Node("child-a", new[]
                {
                    new Node("grandchild", Enumerable.Empty<Node>())
                }),
                new Node("child-b", Enumerable.Empty<Node>())
            })
        };

        string[] flattened = tree.Flatten(node => node.Children).Select(node => node.Name).ToArray();

        Assert.Equal(new[] { "grandchild", "child-a", "child-b", "root" }, flattened);
    }

    private sealed record Node(string Name, IEnumerable<Node> Children);
}

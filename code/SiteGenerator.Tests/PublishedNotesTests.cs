using FluentAssertions;
using Xunit;

namespace SiteGenerator.Tests;

public class PublishedNotesTests
{
    [Fact]
    public void Parse_AcceptsPathsAndSlugs()
    {
        var published = PublishedNotes.Parse("thoughts/git.md\r\npython\nnotes\\docker.md\n");

        published.IsPublished("git").Should().BeTrue();
        published.IsPublished("python").Should().BeTrue();
        published.IsPublished("docker").Should().BeTrue();
        published.IsPublished("inbox").Should().BeFalse();
        published.PublishesEverything.Should().BeFalse();
    }

    [Fact]
    public void Parse_IgnoresBlankLinesAndComments()
    {
        var published = PublishedNotes.Parse("# git\n\n   \n  csharp  \n");

        published.IsPublished("git").Should().BeFalse();
        published.IsPublished("csharp").Should().BeTrue();
    }

    [Fact]
    public void Parse_AlwaysPublishesNotesIndex()
    {
        PublishedNotes.Parse("").IsPublished("index").Should().BeTrue();
    }

    [Fact]
    public void IsPublishedFile_UsesFileNameWithoutExtension()
    {
        var published = PublishedNotes.Parse("git");

        published.IsPublishedFile("git.md").Should().BeTrue();
        published.IsPublishedFile("inbox.md").Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_WithoutFile_PublishesEverything()
    {
        var emptyDir = Directory.CreateTempSubdirectory();
        try
        {
            var published = await PublishedNotes.LoadAsync(emptyDir.FullName);

            published.PublishesEverything.Should().BeTrue();
            published.IsPublished("anything").Should().BeTrue();
        }
        finally
        {
            emptyDir.Delete(true);
        }
    }

    [Fact]
    public async Task LoadAsync_ReadsPublishedTxt()
    {
        var dir = Directory.CreateTempSubdirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(dir.FullName, "published.txt"), "git\n");

            var published = await PublishedNotes.LoadAsync(dir.FullName);

            published.IsPublished("git").Should().BeTrue();
            published.IsPublished("inbox").Should().BeFalse();
        }
        finally
        {
            dir.Delete(true);
        }
    }
}

using NSubstitute;
using SiteGenerator.BacklinksProcessing;
using Xunit;

namespace SiteGenerator.Tests.BacklinkTests;

public class BacklinkCollectorTests
{
    [Fact]
    public async Task CollectBacklinksAsync_ShouldCollectCorrectBacklinks()
    {
        // Arrange
        var folderReader = Substitute.For<IFolderReader>();
        folderReader.GetFileContents(Arg.Any<string>(), "*.md").Returns(GetTestFiles());

        var contentPath = "testPath";

        // Act
        var backlinks = await BacklinkCollector.CollectBacklinksAsync(folderReader, contentPath);

        // Assert
        var backlinksForNote1 = backlinks.GetBacklinksForNote("Note1").ToList();
        Assert.Single(backlinksForNote1);
        Assert.Contains("Note2", backlinksForNote1);

        var backlinksForNote2 = backlinks.GetBacklinksForNote("Note2").ToList();
        Assert.Single(backlinksForNote2);
        Assert.Contains("Note1", backlinksForNote2);
    }

    private static async IAsyncEnumerable<ContentFile> GetTestFiles()
    {
        yield return new ContentFile("Note1.md", "[[Note2]]");
        yield return new ContentFile("Note2.md", "[[Note1]]");
    }
}

using NSubstitute;
using SiteGenerator.BacklinksProcessing;
using SiteGenerator.Tests.Helpers;
using Xunit;

namespace SiteGenerator.Tests.BacklinkTests;

public class BacklinkCollectorTests
{
    [Fact]
    public async Task CollectBacklinksAsync_ShouldCollectCorrectBacklinks()
    {
        // Arrange
        var folderReader = Substitute.For<IFolderReader>();
        var testFiles = new List<ContentFile>
        {
            new ContentFile("Note1.md", "[[Note2]]"),
            new ContentFile("Note2.md", "[[Note1]]")
        };
        folderReader.GetFileContents(Arg.Any<string>(), "*.md").Returns(testFiles.ToIAsyncEnumerable());

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
}

using SiteGenerator.BacklinksProcessing;
using Xunit;

namespace SiteGenerator.Tests.BacklinkTests;

public class BacklinksTests
{
    [Fact]
    public void AddBacklink_ShouldAddCorrectBacklink()
    {
        // Arrange
        var backlinks = new Backlinks();

        // Act
        backlinks.AddBacklink("Note1", "Note2");

        // Assert
        var backlinksForNote1 = backlinks.GetBacklinksForNote("Note1").ToList();
        Assert.Single(backlinksForNote1);
        Assert.Contains("Note2", backlinksForNote1);
    }

    [Fact]
    public void GetBacklinksForNote_ShouldReturnEmptyIfNoBacklinks()
    {
        // Arrange
        var backlinks = new Backlinks();

        // Act
        var result = backlinks.GetBacklinksForNote("NonExistentNote");

        // Assert
        Assert.Empty(result);
    }
}

using FluentAssertions;
using SiteGenerator.Tests.Helpers;
using Xunit;

namespace SiteGenerator.Tests;

public sealed class FolderReaderTests : IDisposable
{
    private const string TestFolderPath = "TestFolder";

    public FolderReaderTests()
    {
        if (!Directory.Exists(TestFolderPath))
        {
            Directory.CreateDirectory(TestFolderPath);
        }
    }

    [Fact]
    public async Task GetFileContents_ShouldReturnFilesWithContent()
    {
        // Arrange: Set up test files in the temporary folder
        string file1Path = Path.Combine(TestFolderPath, "file1.txt");
        string file2Path = Path.Combine(TestFolderPath, "file2.txt");

        await File.WriteAllTextAsync(file1Path, "Content of file 1");
        await File.WriteAllTextAsync(file2Path, "Content of file 2");

        var folderReader = new FolderReader();

        // Act: Read files from the folder
        var files = await folderReader.GetFileContents(TestFolderPath).ToListAsync();

        // Assert: Check that the correct files and contents are returned
        files.Should().HaveCount(2);
        files
            .Should()
            .ContainSingle(f => f.Name == "file1.txt" && f.Content == "Content of file 1");
        files
            .Should()
            .ContainSingle(f => f.Name == "file2.txt" && f.Content == "Content of file 2");
    }

    public void Dispose()
    {
        // Clean up: Delete test files and folder after test
        if (Directory.Exists(TestFolderPath))
        {
            Directory.Delete(TestFolderPath, true);
        }
    }
}

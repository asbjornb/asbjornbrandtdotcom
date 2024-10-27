using FluentAssertions;
using SiteGenerator.Tests.Helpers;
using Xunit;

namespace SiteGenerator.Tests;

public sealed class FileProviderTests : IDisposable
{
    private const string TestFolderPath = "TestFolder";
    private readonly FileProvider _fileProvider;

    public FileProviderTests()
    {
        if (!Directory.Exists(TestFolderPath))
        {
            Directory.CreateDirectory(TestFolderPath);
        }
        _fileProvider = new FileProvider();
    }

    [Fact]
    public async Task GetFileContents_ShouldReturnFilesWithContent()
    {
        // Arrange: Set up test files in the temporary folder
        string file1Path = Path.Combine(TestFolderPath, "file1.txt");
        string file2Path = Path.Combine(TestFolderPath, "file2.txt");

        await File.WriteAllTextAsync(file1Path, "Content of file 1");
        await File.WriteAllTextAsync(file2Path, "Content of file 2");

        var fileProvider = new FileProvider();

        // Act: Read files from the folder
        var files = await fileProvider.GetFileContents(TestFolderPath).ToListAsync();

        // Assert: Check that the correct files and contents are returned
        files.Should().HaveCount(2);
        files
            .Should()
            .ContainSingle(f => f.Name == "file1.txt" && f.Content == "Content of file 1");
        files
            .Should()
            .ContainSingle(f => f.Name == "file2.txt" && f.Content == "Content of file 2");
    }

    [Fact]
    public async Task GetFileContents_WithSearchPattern_ShouldReturnOnlyMatchingFiles()
    {
        // Arrange
        string txtFilePath = Path.Combine(TestFolderPath, "file1.txt");
        string mdFilePath = Path.Combine(TestFolderPath, "file2.md");

        await File.WriteAllTextAsync(txtFilePath, "Content of file 1");
        await File.WriteAllTextAsync(mdFilePath, "Content of file 2");

        var fileProvider = new FileProvider();

        // Act
        var files = await fileProvider.GetFileContents(TestFolderPath, "*.txt").ToListAsync();

        // Assert
        files.Should().HaveCount(1);
        files
            .Should()
            .ContainSingle(f => f.Name == "file1.txt" && f.Content == "Content of file 1");
    }

    [Fact]
    public async Task WriteFileAsync_CreatesDirectoryAndFile()
    {
        // Arrange
        var deepPath = Path.Combine(TestFolderPath, "deep", "nested", "path");
        var filePath = Path.Combine(deepPath, "test.txt");
        var content = "Test content";

        // Act
        await _fileProvider.WriteFileAsync(filePath, content);

        // Assert
        Assert.True(Directory.Exists(deepPath));
        Assert.True(File.Exists(filePath));
        Assert.Equal(content, await File.ReadAllTextAsync(filePath));
    }

    [Fact]
    public async Task WriteFileAsync_OverwritesExistingFile()
    {
        // Arrange
        var filePath = Path.Combine(TestFolderPath, "test.txt");
        var initialContent = "Initial content";
        var newContent = "New content";

        // Act
        await _fileProvider.WriteFileAsync(filePath, initialContent);
        await _fileProvider.WriteFileAsync(filePath, newContent);

        // Assert
        Assert.Equal(newContent, await File.ReadAllTextAsync(filePath));
    }

    public void Dispose()
    {
        if (Directory.Exists(TestFolderPath))
        {
            Directory.Delete(TestFolderPath, true);
        }
    }
}

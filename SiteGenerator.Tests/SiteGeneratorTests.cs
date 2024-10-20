using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SiteGenerator.Tests
{
    public class SiteGeneratorTests : IAsyncLifetime
    {
        private string _testRootPath;
        private string _contentPath;
        private string _outputPath;

        public async Task InitializeAsync()
        {
            _testRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _contentPath = Path.Combine(_testRootPath, "TestContent");
            _outputPath = Path.Combine(_testRootPath, "TestOutput");

            Directory.CreateDirectory(_contentPath);
            await File.WriteAllTextAsync(Path.Combine(_contentPath, "test.md"), "# Test\nThis is a test.");
        }

        public Task DisposeAsync()
        {
            if (Directory.Exists(_testRootPath))
            {
                Directory.Delete(_testRootPath, true);
            }
            return Task.CompletedTask;
        }

        [Fact]
        public async Task GenerateSiteAsync_ShouldCreateHtmlFiles()
        {
            // Arrange
            var generator = new Generator(_contentPath, _outputPath);

            // Act
            await generator.GenerateSiteAsync();

            // Assert
            var outputFile = Path.Combine(_outputPath, "test.html");
            Assert.True(File.Exists(outputFile));

            var content = await File.ReadAllTextAsync(outputFile);
            Assert.Contains("<h1 id=\"test\">Test</h1>", content);
            Assert.Contains("<p>This is a test.</p>", content);
        }
    }
}

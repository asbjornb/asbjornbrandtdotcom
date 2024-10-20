using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SiteGenerator.Tests
{
    public class SiteGeneratorTests
    {
        [Fact]
        public async Task GenerateSiteAsync_ShouldCreateHtmlFiles()
        {
            // Arrange
            var tempContentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempOutputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(tempContentDir);
            File.WriteAllText(Path.Combine(tempContentDir, "test.md"), "# Test\nThis is a test.");

            var generator = new Generator(tempContentDir, tempOutputDir);

            // Act
            await generator.GenerateSiteAsync();

            // Assert
            var outputFile = Path.Combine(tempOutputDir, "test.html");
            Assert.True(File.Exists(outputFile));

            var content = await File.ReadAllTextAsync(outputFile);
            Assert.Contains("<h1 id=\"test\">Test</h1>", content);
            Assert.Contains("<p>This is a test.</p>", content);

            // Cleanup
            Directory.Delete(tempContentDir, true);
            Directory.Delete(tempOutputDir, true);
        }
    }
}

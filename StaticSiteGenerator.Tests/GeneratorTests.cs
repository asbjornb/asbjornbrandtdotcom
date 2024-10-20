using System;
using System.IO;
using Xunit;
using StaticSiteGenerator;

namespace StaticSiteGenerator.Tests
{
    public class GeneratorTests : IDisposable
    {
        private readonly string testContentDir;
        private readonly string testOutputDir;

        public GeneratorTests()
        {
            // Set up test directories
            testContentDir = Path.Combine(Path.GetTempPath(), "TestContent");
            testOutputDir = Path.Combine(Path.GetTempPath(), "TestOutput");
            Directory.CreateDirectory(testContentDir);
            Directory.CreateDirectory(testOutputDir);

            // Create test content structure
            Directory.CreateDirectory(Path.Combine(testContentDir, "pages"));
            Directory.CreateDirectory(Path.Combine(testContentDir, "posts"));
            Directory.CreateDirectory(Path.Combine(testContentDir, "notes"));
        }

        public void Dispose()
        {
            // Clean up test directories
            Directory.Delete(testContentDir, true);
            Directory.Delete(testOutputDir, true);
        }

        [Fact]
        public void TestPageGeneration()
        {
            // Arrange
            var pageContent = "---\ntitle: Test Page\n---\n# Test Page\nThis is a test page.";
            File.WriteAllText(Path.Combine(testContentDir, "pages", "test-page.md"), pageContent);

            var generator = new Generator(testContentDir, testOutputDir);

            // Act
            generator.GenerateSite();

            // Assert
            var outputFile = Path.Combine(testOutputDir, "pages", "test-page.html");
            Assert.True(File.Exists(outputFile));
            var outputContent = File.ReadAllText(outputFile);
            Assert.Contains("<h1>Test Page</h1>", outputContent);
            Assert.Contains("<p>This is a test page.</p>", outputContent);
        }

        [Fact]
        public void TestNoteWithObsidianLinks()
        {
            // Arrange
            var note1Content = "# Note 1\nThis links to [[Note 2]].";
            var note2Content = "# Note 2\nThis is referenced by Note 1.";
            File.WriteAllText(Path.Combine(testContentDir, "notes", "note-1.md"), note1Content);
            File.WriteAllText(Path.Combine(testContentDir, "notes", "note-2.md"), note2Content);

            var generator = new Generator(testContentDir, testOutputDir);

            // Act
            generator.GenerateSite();

            // Assert
            var outputFile1 = Path.Combine(testOutputDir, "notes", "note-1.html");
            var outputFile2 = Path.Combine(testOutputDir, "notes", "note-2.html");
            Assert.True(File.Exists(outputFile1));
            Assert.True(File.Exists(outputFile2));

            var outputContent1 = File.ReadAllText(outputFile1);
            Assert.Contains("<a href=\"note-2.html\">Note 2</a>", outputContent1);
        }
    }
}

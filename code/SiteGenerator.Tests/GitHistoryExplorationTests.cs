using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace SiteGenerator.Tests;

public class GitHistoryExplorationTests
{
    private readonly ITestOutputHelper _output;

    public GitHistoryExplorationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Exploration test to understand git log performance and completeness with different commit limits.
    /// This helps determine optimal settings for GitHistoryService, not testing our actual code.
    /// Automatically skipped in CI environments to avoid slow builds.
    /// </summary>
    [SkipInCITheory]
    [InlineData("A", 30)] // Added files, last 30 commits
    [InlineData("A", 50)] // Added files, last 50 commits
    [InlineData("A", 100)] // Added files, last 100 commits
    [InlineData("A", 200)] // Added files, last 200 commits
    [InlineData("M", 30)] // Modified files, last 30 commits
    [InlineData("M", 50)] // Modified files, last 50 commits
    [InlineData("M", 100)] // Modified files, last 100 commits
    [InlineData("M", 200)] // Modified files, last 200 commits
    [Trait("Category", "Exploration")]
    public async Task GitLog_ExploreCommitLimitsForOptimalSettings(
        string diffFilter,
        int commitLimit
    )
    {
        // Arrange
        var basePath = GetProjectRoot();
        var thoughtsPath = Path.Combine(basePath, "content", "thoughts");
        var filterName = diffFilter == "A" ? "Added" : "Modified";
        var gitCommand =
            $"log --name-only --pretty=format: --diff-filter={diffFilter} -{commitLimit} -- \"{thoughtsPath}/*.md\"";

        // Act
        var stopwatch = Stopwatch.StartNew();
        var (success, output, error) = await RunGitCommand(gitCommand, basePath);
        stopwatch.Stop();

        // Assert & Log
        if (success)
        {
            var fileCount = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
            _output.WriteLine(
                $"{filterName} (-{commitLimit}): {stopwatch.ElapsedMilliseconds}ms → {fileCount} files"
            );
        }
        else
        {
            _output.WriteLine(
                $"{filterName} (-{commitLimit}): FAILED after {stopwatch.ElapsedMilliseconds}ms - {error}"
            );
        }
    }

    private async Task<(bool Success, string Output, string Error)> RunGitCommand(
        string arguments,
        string workingDirectory
    )
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return (process.ExitCode == 0, output, error);
    }

    private static string GetProjectRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var searchDir = currentDir;

        while (searchDir != null)
        {
            if (
                Directory.Exists(Path.Combine(searchDir, "content"))
                && Directory.Exists(Path.Combine(searchDir, "code"))
            )
            {
                return searchDir;
            }

            var parentDir = Directory.GetParent(searchDir);
            if (parentDir == null)
                break;
            searchDir = parentDir.FullName;
        }

        throw new DirectoryNotFoundException($"Could not find project root from: {currentDir}");
    }
}

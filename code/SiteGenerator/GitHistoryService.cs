using System.Diagnostics;

namespace SiteGenerator;

public class GitHistoryService
{
    private readonly string _contentPath;

    public GitHistoryService(string contentPath)
    {
        _contentPath = contentPath;
    }

    public async Task<List<string>> GetRecentNotesAsync(int count = 5)
    {
        var thoughtsPath = Path.Combine(_contentPath, "thoughts");
        var gitCommand =
            $"log --name-only --pretty=format: --diff-filter=A -50 -- \"{thoughtsPath}/*.md\"";
        var output = await RunGitCommandAsync(gitCommand);

        return ParseFileNames(output, count);
    }

    public async Task<List<string>> GetRecentlyChangedNotesAsync(int count = 5)
    {
        var thoughtsPath = Path.Combine(_contentPath, "thoughts");
        // Exclude newly added files to avoid duplicates with newest notes
        var gitCommand =
            $"log --name-only --pretty=format: --diff-filter=M -30 -- \"{thoughtsPath}/*.md\"";
        var output = await RunGitCommandAsync(gitCommand);

        return ParseFileNames(output, count);
    }

    private async Task<string> RunGitCommandAsync(string arguments)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = _contentPath,
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

        if (process.ExitCode != 0)
        {
            throw new Exception($"Git command failed: {error}");
        }

        return output;
    }

    private List<string> ParseFileNames(string gitOutput, int count)
    {
        var lines = gitOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var fileNames = new List<string>();
        var seen = new HashSet<string>();

        foreach (var line in lines)
        {
            if (line.EndsWith(".md") && line.Contains("thoughts/"))
            {
                var fileName = Path.GetFileNameWithoutExtension(Path.GetFileName(line.Trim()));

                // Skip index.md, empty names, and ensure uniqueness
                if (
                    !string.IsNullOrEmpty(fileName)
                    && !fileName.Equals("index", StringComparison.OrdinalIgnoreCase)
                    && seen.Add(fileName)
                )
                {
                    fileNames.Add(fileName);

                    if (fileNames.Count >= count)
                        break;
                }
            }
        }

        return fileNames;
    }
}

using System.Diagnostics;
using System.Text.Json;

namespace SiteGenerator.GitHistory;

public class GitHistoryProvider
{
    private readonly string _contentPath;

    public GitHistoryProvider(string contentPath)
    {
        _contentPath = contentPath;
    }

    public async Task<RecentFiles> GetRecentFilesAsync()
    {
        try
        {
            var recentlyAdded = await GetRecentlyAddedFilesAsync();
            var recentlyModified = await GetRecentlyModifiedFilesAsync();

            return new RecentFiles(recentlyAdded, recentlyModified);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get git history: {ex.Message}");
            // Return empty lists if git fails
            return new RecentFiles(new List<FileHistoryInfo>(), new List<FileHistoryInfo>());
        }
    }

    private async Task<List<FileHistoryInfo>> GetRecentlyAddedFilesAsync()
    {
        var command = "log --diff-filter=A --name-only --pretty=format:%H|%ci -- content/thoughts/*.md";
        var output = await RunGitCommandAsync(command);
        return ParseGitLogOutput(output).Take(5).ToList();
    }

    private async Task<List<FileHistoryInfo>> GetRecentlyModifiedFilesAsync()
    {
        var command = "log -10 --name-only --pretty=format:%H|%ci -- content/thoughts/*.md";
        var output = await RunGitCommandAsync(command);

        // Group by filename and take the most recent modification for each file
        var fileGroups = ParseGitLogOutput(output)
            .GroupBy(f => f.FileName)
            .Select(g => g.OrderByDescending(f => f.Date).First())
            .OrderByDescending(f => f.Date)
            .Take(5)
            .ToList();

        return fileGroups;
    }

    private async Task<string> RunGitCommandAsync(string command)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = command,
                WorkingDirectory = Path.GetDirectoryName(_contentPath) ?? _contentPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        try
        {
            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            var timeoutTask = Task.Delay(5000); // 5 second timeout
            var processTask = process.WaitForExitAsync();

            var completedTask = await Task.WhenAny(processTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                process.Kill();
                throw new TimeoutException("Git command timed out");
            }

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Git command failed: {error}");
            }

            return output;
        }
        catch (Exception ex)
        {
            // Return empty string on any error to gracefully handle git issues
            Console.WriteLine($"Git command failed: {ex.Message}");
            return string.Empty;
        }
    }

    private static List<FileHistoryInfo> ParseGitLogOutput(string output)
    {
        var results = new List<FileHistoryInfo>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        string? currentHash = null;
        DateTime currentDate = default;

        foreach (var line in lines)
        {
            if (line.Contains('|'))
            {
                // This is a commit info line: hash|date
                var parts = line.Split('|');
                if (parts.Length == 2)
                {
                    currentHash = parts[0];
                    if (DateTime.TryParse(parts[1], out var date))
                    {
                        currentDate = date;
                    }
                }
            }
            else if (
                !string.IsNullOrWhiteSpace(line)
                && line.StartsWith("content/thoughts/")
                && line.EndsWith(".md")
            )
            {
                // This is a filename
                var fileName = Path.GetFileNameWithoutExtension(Path.GetFileName(line));
                results.Add(new FileHistoryInfo(fileName, currentDate, currentHash ?? ""));
            }
        }

        return results;
    }
}

public record FileHistoryInfo(string FileName, DateTime Date, string CommitHash)
{
    public string FormattedDate => Date.ToString("MMM d");
};

public record RecentFiles(
    IReadOnlyList<FileHistoryInfo> RecentlyAdded,
    IReadOnlyList<FileHistoryInfo> RecentlyModified
);

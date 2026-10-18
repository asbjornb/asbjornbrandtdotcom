namespace SiteGenerator.Configuration;

public class PathResolver
{
    private static readonly string SolutionMarker = Path.Combine("code", "SiteGenerator.sln");

    public static string ResolveBasePath(string? configuredBasePath = null)
    {
        // If a valid absolute path is provided via configuration, use it
        if (
            !string.IsNullOrEmpty(configuredBasePath)
            && Path.IsPathFullyQualified(configuredBasePath)
            && Directory.Exists(configuredBasePath)
        )
        {
            return configuredBasePath;
        }

        // Auto-detect: the repo root is the first directory upwards that holds code/SiteGenerator.sln.
        // Content may live elsewhere, so it is not used as a marker.
        var currentDir = Directory.GetCurrentDirectory();
        for (var dir = new DirectoryInfo(currentDir); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, SolutionMarker)))
            {
                return dir.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not auto-detect project root. Searched upwards from: {currentDir}. "
                + "Please ensure you're running from inside the repository or provide an absolute BasePath in configuration."
        );
    }
}

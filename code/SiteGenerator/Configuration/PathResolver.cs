namespace SiteGenerator.Configuration;

public class PathResolver
{
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

        // Auto-detect by looking for project markers
        var currentDir = Directory.GetCurrentDirectory();
        var searchDir = currentDir;

        // Look for common project markers to identify the project root
        string[] projectMarkers =
        {
            "SiteGenerator.sln",
            ".git",
            "content", // Our content directory
            "README.md",
        };

        while (searchDir != null)
        {
            // Check if any of the project markers exist in this directory
            foreach (var marker in projectMarkers)
            {
                var markerPath = Path.Combine(searchDir, marker);
                if (File.Exists(markerPath) || Directory.Exists(markerPath))
                {
                    // If we found a solution file, the project root is here
                    if (marker == "SiteGenerator.sln")
                    {
                        return Path.GetDirectoryName(searchDir) ?? searchDir;
                    }
                    // For other markers, check if this looks like our project structure
                    var contentPath = Path.Combine(searchDir, "content");
                    var codePath = Path.Combine(searchDir, "code");
                    if (Directory.Exists(contentPath) && Directory.Exists(codePath))
                    {
                        return searchDir;
                    }
                }
            }

            // Move up one directory level
            var parentDir = Directory.GetParent(searchDir);
            if (parentDir == null)
                break;
            searchDir = parentDir.FullName;
        }

        // Fallback: if we can't auto-detect, try some common relative paths
        var fallbackPaths = new[]
        {
            Path.GetFullPath(Path.Combine(currentDir, "../../../..")), // From bin/Debug/net9.0
            Path.GetFullPath(Path.Combine(currentDir, "../../..")), // From bin/Debug
            Path.GetFullPath(Path.Combine(currentDir, "../..")), // From SiteGenerator
            currentDir // Last resort
            ,
        };

        foreach (var fallbackPath in fallbackPaths)
        {
            var contentCheck = Path.Combine(fallbackPath, "content");
            var codeCheck = Path.Combine(fallbackPath, "code");
            if (Directory.Exists(contentCheck) && Directory.Exists(codeCheck))
            {
                return fallbackPath;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not auto-detect project root. Searched from: {currentDir}. "
                + "Please ensure you're running from the project directory or provide an absolute BasePath in configuration."
        );
    }
}

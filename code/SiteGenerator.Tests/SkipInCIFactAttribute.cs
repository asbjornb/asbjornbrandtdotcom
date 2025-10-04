using Xunit;

namespace SiteGenerator.Tests;

/// <summary>
/// Custom Fact attribute that automatically skips tests when running in CI environments.
/// Detects common CI environment variables from GitHub Actions, Azure DevOps, etc.
/// </summary>
public sealed class SkipInCIFactAttribute : FactAttribute
{
    public SkipInCIFactAttribute()
    {
        if (IsRunningInCI())
        {
            Skip = "Skipped when running in CI environment";
        }
    }

    private static bool IsRunningInCI()
    {
        // Check for common CI environment variables
        var ciEnvVars = new[]
        {
            "CI", // Generic CI indicator
            "GITHUB_ACTIONS", // GitHub Actions
            "AZURE_PIPELINES", // Azure DevOps
            "BUILD_BUILDID", // Azure DevOps
            "JENKINS_URL", // Jenkins
            "TRAVIS", // Travis CI
            "CIRCLECI", // CircleCI
            "APPVEYOR", // AppVeyor
            "TEAMCITY_VERSION" // TeamCity
            ,
        };

        return ciEnvVars.Any(envVar =>
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(envVar))
        );
    }
}

/// <summary>
/// Custom Theory attribute that automatically skips tests when running in CI environments.
/// Detects common CI environment variables from GitHub Actions, Azure DevOps, etc.
/// </summary>
public sealed class SkipInCITheoryAttribute : TheoryAttribute
{
    public SkipInCITheoryAttribute()
    {
        if (IsRunningInCI())
        {
            Skip = "Skipped when running in CI environment";
        }
    }

    private static bool IsRunningInCI()
    {
        // Check for common CI environment variables
        var ciEnvVars = new[]
        {
            "CI", // Generic CI indicator
            "GITHUB_ACTIONS", // GitHub Actions
            "AZURE_PIPELINES", // Azure DevOps
            "BUILD_BUILDID", // Azure DevOps
            "JENKINS_URL", // Jenkins
            "TRAVIS", // Travis CI
            "CIRCLECI", // CircleCI
            "APPVEYOR", // AppVeyor
            "TEAMCITY_VERSION" // TeamCity
            ,
        };

        return ciEnvVars.Any(envVar =>
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(envVar))
        );
    }
}

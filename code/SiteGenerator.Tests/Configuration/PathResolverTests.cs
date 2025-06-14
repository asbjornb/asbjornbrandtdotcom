using FluentAssertions;
using SiteGenerator.Configuration;
using Xunit;

namespace SiteGenerator.Tests.Configuration;

public class PathResolverTests
{
    [Fact]
    public void ResolveBasePath_WithValidAbsolutePath_ReturnsConfiguredPath()
    {
        // Arrange
        var validPath = Directory.GetCurrentDirectory();

        // Act
        var result = PathResolver.ResolveBasePath(validPath);

        // Assert
        result.Should().Be(validPath);
    }

    [Fact]
    public void ResolveBasePath_WithEmptyPath_AutoDetectsProjectRoot()
    {
        // Act
        var result = PathResolver.ResolveBasePath("");

        // Assert
        result.Should().NotBeNullOrEmpty();
        Directory.Exists(Path.Combine(result, "content")).Should().BeTrue();
        Directory.Exists(Path.Combine(result, "code")).Should().BeTrue();
    }

    [Fact]
    public void ResolveBasePath_WithNullPath_AutoDetectsProjectRoot()
    {
        // Act
        var result = PathResolver.ResolveBasePath(null);

        // Assert
        result.Should().NotBeNullOrEmpty();
        Directory.Exists(Path.Combine(result, "content")).Should().BeTrue();
        Directory.Exists(Path.Combine(result, "code")).Should().BeTrue();
    }

    [Fact]
    public void ResolveBasePath_WithInvalidPath_AutoDetectsProjectRoot()
    {
        // Arrange
        var invalidPath = "/this/path/does/not/exist";

        // Act
        var result = PathResolver.ResolveBasePath(invalidPath);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().NotBe(invalidPath);
        Directory.Exists(Path.Combine(result, "content")).Should().BeTrue();
        Directory.Exists(Path.Combine(result, "code")).Should().BeTrue();
    }
}

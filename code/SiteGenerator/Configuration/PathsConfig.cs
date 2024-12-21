namespace SiteGenerator.Configuration;

public record PathsConfig(
    string BasePath,
    string ContentDirectory,
    string OutputDirectory,
    string TemplateDirectory
);

namespace SiteGenerator.Configuration;

public record PathsConfig(
    string ContentDirectory,
    string OutputDirectory,
    string TemplateDirectory
);

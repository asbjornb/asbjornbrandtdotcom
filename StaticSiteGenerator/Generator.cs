using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json;

public class Generator
{
    private readonly string contentDir;
    private readonly string outputDir;
    private Config config;
    private Dictionary<string, List<string>> backlinks;
    private Dictionary<string, string> noteMapping;

    public Generator(string contentDir, string outputDir)
    {
        this.contentDir = contentDir;
        this.outputDir = outputDir;
        string configContent = File.ReadAllText(Path.Combine(contentDir, "..", "config", "config.json"));
        config = JsonConvert.DeserializeObject<Config>(configContent);
        backlinks = new Dictionary<string, List<string>>();
        noteMapping = new Dictionary<string, string>();
    }

    // ... (update other methods to use contentDir and outputDir)
}

using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Config
{
    public string SiteTitle { get; set; }
    public string BaseUrl { get; set; }
}

public class Generator
{
    private Config config;
    private Dictionary<string, List<string>> backlinks;
    private Dictionary<string, string> noteMapping;

    public Generator()
    {
        string configContent = File.ReadAllText("../config/config.json");
        config = JsonConvert.DeserializeObject<Config>(configContent);
        backlinks = new Dictionary<string, List<string>>();
        noteMapping = new Dictionary<string, string>();
    }

    public void GenerateSite()
    {
        BuildNoteMapping();
        ProcessPages();
        ProcessPosts();
        ProcessNotes();
        GenerateIndexPages();
        CopyAssets();
    }

    private void BuildNoteMapping()
    {
        var noteFiles = Directory.GetFiles("../content/notes/", "*.md");
        foreach (var file in noteFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            noteMapping[fileName] = $"notes/{fileName}.html";
        }
    }

    private void ProcessPages()
    {
        var pageFiles = Directory.GetFiles("../content/pages/", "*.md");
        foreach (var file in pageFiles)
        {
            ProcessContentFile(file, "page");
        }
    }

    private void ProcessPosts()
    {
        var postFiles = Directory.GetFiles("../content/posts/", "*.md");
        foreach (var file in postFiles)
        {
            ProcessContentFile(file, "post");
        }
    }

    private void ProcessNotes()
    {
        var noteFiles = Directory.GetFiles("../content/notes/", "*.md");
        foreach (var file in noteFiles)
        {
            ProcessContentFile(file, "note");
        }
    }

    private void ProcessContentFile(string filePath, string contentType)
    {
        // Implementation for processing individual content files
        // This method will be implemented in the next iteration
    }

    private void GenerateIndexPages()
    {
        // Implementation for generating index pages
        // This method will be implemented in the next iteration
    }

    private void CopyAssets()
    {
        Directory.CreateDirectory("../output/assets");
        foreach (var file in Directory.GetFiles("../assets"))
        {
            File.Copy(file, Path.Combine("../output/assets", Path.GetFileName(file)), true);
        }
    }
}

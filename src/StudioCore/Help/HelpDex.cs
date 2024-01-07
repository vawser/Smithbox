using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StudioCore.Help;

public class HelpDex
{
    private readonly TextEntry _credits;
    private readonly List<HelpEntry> _helpArticles;
    private readonly List<HelpEntry> _helpGlossary;
    private readonly List<HelpEntry> _helpTutorials;
    private readonly LinkEntries _links;

    public HelpDex()
    {
        _helpArticles = LoadHelpResource("Articles");
        _helpTutorials = LoadHelpResource("Tutorials");
        _helpGlossary = LoadHelpResource("Glossary");
        _links = JsonSerializer.Deserialize<LinkEntries>(
            File.OpenRead(AppContext.BaseDirectory + @"\Assets\Help\Links.json"),
            LinkEntriesSerializerContext.Default.LinkEntries);
        _credits = JsonSerializer.Deserialize<TextEntry>(
            File.OpenRead(AppContext.BaseDirectory + @"\Assets\Help\Credits.json"),
            TextEntrySerializerContext.Default.TextEntry);
    }

    private List<HelpEntry> LoadHelpResource(string directory)
    {
        List<HelpEntry> helpEntries = new();

        IEnumerable<string> articleDirFiles =
            from file in Directory.EnumerateFiles(AppContext.BaseDirectory + $@"\Assets\Help\{directory}\")
            select file;
        foreach (var file in articleDirFiles)
        {
            helpEntries.Add(LoadHelpJSON(file));
        }

        return helpEntries;
    }

    private HelpEntry LoadHelpJSON(string path)
    {
        HelpEntry resource = new();

        if (File.Exists(path))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = HelpEntrySerializerContext.Default
            };
            resource = JsonSerializer.Deserialize(File.OpenRead(path), typeof(HelpEntry), options) as HelpEntry;
        }

        return resource;
    }

    public List<HelpEntry> GetArticles()
    {
        return _helpArticles;
    }

    public List<HelpEntry> GetTutorials()
    {
        return _helpTutorials;
    }

    public List<HelpEntry> GetGlossaryEntries()
    {
        return _helpGlossary;
    }

    public List<LinkEntry> GetLinks()
    {
        return _links.Links;
    }

    public TextEntry GetCredits()
    {
        return _credits;
    }
}

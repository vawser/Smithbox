using StudioCore.Help;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StudioCore.Banks.HelpBank;

public class HelpBank
{
    private List<HelpEntry> _helpArticles;
    private List<HelpEntry> _helpGlossary;
    private List<HelpEntry> _helpTutorials;
    private List<HelpEntry> _helpLinks;
    private List<HelpEntry> _helpCredits;
    private List<HelpEntry> _helpMassEdit;
    private List<HelpEntry> _helpRegex;

    public HelpBank()
    {
        _helpArticles = LoadHelpResource("Articles");
        _helpTutorials = LoadHelpResource("Tutorials");
        _helpGlossary = LoadHelpResource("Glossary");
        _helpMassEdit = LoadHelpResource("MassEdit");
        _helpRegex = LoadHelpResource("Regex");
        _helpLinks = LoadHelpResource("Links");
        _helpCredits = LoadHelpResource("Credits");
    }

    private List<HelpEntry> LoadHelpResource(string directory)
    {
        List<HelpEntry> helpEntries = new();

        IEnumerable<string> articleDirFiles =
            from file in Directory.EnumerateFiles(AppContext.BaseDirectory + $@"\Assets\Help\{directory}\")
            select file;
        foreach (var file in articleDirFiles)
            helpEntries.Add(LoadHelpJSON(file));

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

    public List<HelpEntry> GetLinks()
    {
        return _helpLinks;
    }

    public List<HelpEntry> GetCredits()
    {
        return _helpCredits;
    }

    public List<HelpEntry> GetMassEditHelp()
    {
        return _helpMassEdit;
    }

    public List<HelpEntry> GetRegexHelp()
    {
        return _helpRegex;
    }
}

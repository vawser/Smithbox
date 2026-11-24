using StudioCore.Application;
using StudioCore.Editors.TextEditor;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SmithboxTests;

/// <summary>
///     Every game's FMG Descriptor Registry (Smithbox.Data/Assets/FMG/{game}), which the Text Editor and its
///     preferences read the languages and text containers from. A game without one has no primary language to pick.
/// </summary>
public class FmgDescriptorRegistryTests
{
    public static TheoryData<ProjectType> Games()
    {
        var data = new TheoryData<ProjectType>();

        foreach (var type in Enum.GetValues<ProjectType>())
        {
            if (type is not ProjectType.Undefined)
                data.Add(type);
        }

        return data;
    }

    private static string AssetsFolder()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var assets = Path.Combine(dir.FullName, "Smithbox.Data", "Assets");

            if (Directory.Exists(assets))
                return assets;
        }

        throw new DirectoryNotFoundException("Smithbox.Data/Assets wasn't found above " + AppContext.BaseDirectory);
    }

    private static FmgDescriptors Read(ProjectType type, out string file)
    {
        file = Path.Combine(AssetsFolder(), "FMG", ProjectUtils.GetGameDirectory(type), "FMG Descriptor Registry.json");
        Assert.True(File.Exists(file), $"{type} has no FMG Descriptor Registry ({file})");

        return JsonSerializer.Deserialize<FmgDescriptors>(File.ReadAllText(file))!;
    }

    private static readonly Lazy<HashSet<string>> EnglishKeys = new(() =>
        Directory.GetFiles(Path.Combine(AssetsFolder(), "Localization", "English"), "*.json")
            .SelectMany(f => Regex.Matches(File.ReadAllText(f), "\"Key\"\\s*:\\s*\"([^\"]+)\"").Select(m => m.Groups[1].Value))
            .ToHashSet());

    /// <summary>What the Text Editor reads: the languages (with their display names) and the containers</summary>
    [Theory]
    [MemberData(nameof(Games))]
    public void Registry_HasTheLanguagesAndContainers(ProjectType type)
    {
        var registry = Read(type, out var file);

        Assert.NotEmpty(registry.Languages);
        foreach (var language in registry.Languages)
            Assert.True(EnglishKeys.Value.Contains(language.LanguageKey), $"{file}: no localization for {language.LanguageKey}");

        Assert.NotEmpty(registry.Containers);
    }

    /// <summary>
    ///     The rest of a registry, for the Duskbloods' (made from the network test's own files). Not yet true of every
    ///     older registry: DS2S's and SDT's name another game, and several list FMGs under containers they don't
    ///     declare (ER/NR Spells, AC6/DS3 NgWord, DS2 bloodmessageconjunction). Nothing reads those fields today.
    /// </summary>
    [Theory]
    [InlineData(ProjectType.DB_NT)]
    [InlineData(ProjectType.DB)]
    public void Registry_IsConsistent(ProjectType type)
    {
        var registry = Read(type, out var file);

        Assert.Equal(type, registry.ProjectType);

        var containerNames = registry.Containers.Select(c => c.ContainerName).ToHashSet();

        foreach (var fmg in registry.List)
            Assert.True(containerNames.Contains(fmg.Container), $"{file}: FMG {fmg.ID} is in container {fmg.Container}, which isn't listed");

        Assert.Equal(registry.List.Count, registry.List.Select(f => (f.Container, f.ID)).Distinct().Count());
    }

    /// <summary>The full release's registry starts as a copy of the network test's</summary>
    [Fact]
    public void DuskbloodsRegistries_MatchButForTheGame()
    {
        var nt = Read(ProjectType.DB_NT, out var ntFile);
        var db = Read(ProjectType.DB, out var dbFile);

        Assert.Equal(
            File.ReadAllText(ntFile).Replace("\"ProjectType\": 16,", ""),
            File.ReadAllText(dbFile).Replace("\"ProjectType\": 17,", ""));

        Assert.Equal(83, nt.List.Count);
        Assert.Equal(nt.List.Count, db.List.Count);
    }
}

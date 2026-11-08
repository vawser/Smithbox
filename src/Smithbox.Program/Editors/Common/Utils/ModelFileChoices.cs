using StudioCore.Application;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.Common;

/// <summary>
/// A model that exists in the project's files, for a list to pick from.
/// </summary>
/// <param name="ID">The file's name (c1000, o0000, aeg099_001, m10_00_00_00_000100), which the previews load by</param>
/// <param name="ModelName">The name an MSB part uses for it (m000100 for that map piece)</param>
/// <param name="Alias">The alias name, if the alias list has one</param>
/// <param name="Tags">The alias tags, if any</param>
public sealed record ModelChoice(string ID, string ModelName, string Alias, List<string> Tags);

/// <summary>
/// The models a list can offer, read from the project's file lists (<see cref="ProjectFileLocator" />) rather than the
/// alias lists, which only name the models someone has written an alias for. Aliases still give the display names.
/// </summary>
public static partial class ModelFileChoices
{
    [GeneratedRegex(@"^c\d{4}$", RegexOptions.IgnoreCase)]
    private static partial Regex CharacterIdRegex();

    [GeneratedRegex(@"A\d\d$")]
    private static partial Regex Ds1AreaSuffixRegex();

    public static List<ModelChoice> Characters(ProjectEntry project)
    {
        // Only the character binders themselves, not c0000_l style variants or the DS2 bnds of other kinds
        return Build(project, project.Locator.ChrFiles.Entries
            .Select(e => e.Filename)
            .Where(n => CharacterIdRegex().IsMatch(n)), ProjectAliasType.Characters, n => n);
    }

    public static List<ModelChoice> Assets(ProjectEntry project)
    {
        return Build(project, project.Locator.AssetFiles.Entries.Select(e => e.Filename), ProjectAliasType.Assets, n => n);
    }

    /// <summary>
    /// The map pieces a map can use: those in its asset folder (the area's for DS1 and DS1R, which name them for the
    /// area, m0000B0A10). Empty for DS2, which keeps a map's pieces inside one mapbhd.
    /// </summary>
    public static List<ModelChoice> MapPieces(ProjectEntry project, string mapId)
    {
        if (string.IsNullOrEmpty(mapId) || project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            return new();

        var assetMapId = PathBuilder.GetAssetMapID(project, mapId);
        var isDs1 = project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R;
        var areaSuffix = mapId.Length >= 3 ? $"A{mapId.Substring(1, 2)}" : "";

        var names = project.Locator.MapPieceFiles.Entries
            .Where(e => e.Folder.TrimEnd('/').EndsWith("/" + assetMapId, StringComparison.OrdinalIgnoreCase))
            .Select(e => e.Filename)
            .Where(n => !isDs1 || n.EndsWith(areaSuffix, StringComparison.OrdinalIgnoreCase));

        return Build(project, names, ProjectAliasType.MapPieces, n => GetMapPieceModelName(project, assetMapId, n));
    }

    /// <summary>
    /// The MSB model name of a map piece file: m10_00_00_00_000100 is m000100, DS1's m0000B0A10 is m0000B0, and DES's
    /// m0000b0 is itself
    /// </summary>
    public static string GetMapPieceModelName(ProjectEntry project, string assetMapId, string fileName)
    {
        if (project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            return Ds1AreaSuffixRegex().Replace(fileName, "");

        if (fileName.StartsWith($"{assetMapId}_", StringComparison.OrdinalIgnoreCase))
            return "m" + fileName.Substring(assetMapId.Length + 1);

        return fileName;
    }

    /// <summary>
    /// Characters for a value picker, as alias entries: the ID is the file name (c1000), the name its alias
    /// </summary>
    public static List<AliasEntry> CharacterAliasEntries(ProjectEntry project)
    {
        // Pickers ask every frame they are open, so this is kept until the files or aliases change
        project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Characters, out List<AliasEntry> aliases);
        var key = (project.Locator.ChrFiles.Entries.Count, aliases, aliases?.Count ?? 0, aliases?.Sum(a => a.Name?.Length ?? 0) ?? 0);

        var cached = _characterEntries.GetValue(project, _ => new CachedEntries());
        if (cached.Entries == null || !cached.Key.Equals(key))
        {
            cached.Key = key;
            cached.Entries = Characters(project)
                .Select(c => new AliasEntry { ID = c.ID, Name = c.Alias, Tags = c.Tags })
                .ToList();
        }

        return cached.Entries;
    }

    private sealed class CachedEntries
    {
        public (int, List<AliasEntry>, int, int) Key;
        public List<AliasEntry> Entries;
    }

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ProjectEntry, CachedEntries> _characterEntries = new();

    private static List<ModelChoice> Build(ProjectEntry project, IEnumerable<string> fileNames, ProjectAliasType aliasType,
        Func<string, string> modelName)
    {
        var aliases = new Dictionary<string, AliasEntry>(StringComparer.OrdinalIgnoreCase);

        if (project.Handler.ProjectData.Aliases.TryGetValue(aliasType, out List<AliasEntry> list) && list != null)
        {
            foreach (var alias in list)
            {
                if (alias?.ID != null)
                    aliases.TryAdd(alias.ID, alias);
            }
        }

        // A file can be listed more than once (compressed and not, project and game)
        return fileNames
            .Where(n => !string.IsNullOrEmpty(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .Select(n =>
            {
                var model = modelName(n);

                // Map piece aliases may be keyed by either name
                aliases.TryGetValue(n, out var alias);
                if (alias == null)
                    aliases.TryGetValue(model, out alias);

                return new ModelChoice(n, model, alias?.Name ?? "", alias?.Tags ?? new List<string>());
            })
            .ToList();
    }
}

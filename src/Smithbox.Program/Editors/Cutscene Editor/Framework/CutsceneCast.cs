#nullable enable
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>The map part an actor of a cutscene is: where its look (NpcParam masks, CharaInit equipment) is set.</summary>
public sealed record CutsceneCastEntry(string Map, string PartName, int NpcParamId, int CharaInitId);

/// <summary>
/// Finds the map part each character actor of a cutscene stands for, by its name (Claude/Tools/CutsceneActorProbe):
/// <list type="bullet">
/// <item>The same name in DS3, SDT and NR (DS3 c0000_0009 is the enemy c0000_0009, CharaInit 27700).</item>
/// <item>In ER, the part of the 9000 block: c4750_0000 is c4750_9000 (Godrick, NpcParam 47500014). 50 of ER's actors
/// resolve that way, 6 by the same name.</item>
/// </list>
/// The maps looked in are the one shown behind the cutscene, then every map of its area and block (sAA_BB_* →
/// mAA_BB_*). The player (c0000_0000) is no part: it is dressed by the Character Preview tool. MSBs are read once
/// per project and map, from the Map Editor's copy when it has one. Safe to call off the main thread.
/// </summary>
public static class CutsceneCast
{
    private static readonly ConditionalWeakTable<ProjectEntry, ConcurrentDictionary<string, IMsb?>> Msbs = new();

    /// <summary>The names a part of an actor can have in a map, in the order tried</summary>
    public static IEnumerable<string> PartNames(string actorName)
    {
        yield return actorName;

        // ER's 9000 block: cXXXX_0NNN → cXXXX_9NNN
        if (Regex.IsMatch(actorName, @"^c\d{4}_\d{4}$") && actorName[6] != '9')
            yield return $"{actorName[..6]}9{actorName[7..]}";
    }

    /// <summary>The maps a cutscene's actors are looked for in: the one shown, then those of its area and block</summary>
    public static List<string> CandidateMaps(ProjectEntry project, string cutsceneName, string? shownMap)
    {
        var maps = project.Handler.MapData?.PrimaryBank?.Maps?.Keys.Select(k => k.Filename).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                   ?? new List<string>();

        var list = new List<string>();
        if (shownMap != null && maps.Contains(shownMap, StringComparer.OrdinalIgnoreCase))
            list.Add(shownMap);

        var m = Regex.Match(cutsceneName, @"^s(\d\d)_(\d\d)_");
        if (m.Success)
        {
            var prefix = $"m{m.Groups[1].Value}_{m.Groups[2].Value}_";
            list.AddRange(maps.Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                                          !list.Contains(x, StringComparer.OrdinalIgnoreCase))
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        }

        return list;
    }

    /// <summary>The enemy part an actor stands for, or null (the player, props, or a character no map places)</summary>
    public static CutsceneCastEntry? Find(ProjectEntry project, IReadOnlyList<string> maps, string actorName)
    {
        if (!Regex.IsMatch(actorName, @"^c\d{4}_"))
            return null;

        foreach (var name in PartNames(actorName))
        {
            foreach (var map in maps)
            {
                var msb = Msb(project, map);
                var part = msb?.Parts.GetEntries().FirstOrDefault(p =>
                    p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && p.GetType().Name.Contains("Enemy"));

                if (part != null)
                    return new CutsceneCastEntry(map, part.Name, Id(part, "NPCParamID", "NpcParamId"), Id(part, "CharaInitID", "CharaInitParamId"));
            }
        }

        return null;
    }

    // The NpcParam and CharaInit fields go by other names in each format (NR: NpcParamId, CharaInitParamId)
    private static int Id(object part, params string[] names)
    {
        foreach (var name in names)
        {
            if (part.GetType().GetProperty(name)?.GetValue(part) is { } v)
                return Convert.ToInt32(v);
        }

        return -1;
    }

    private static IMsb? Msb(ProjectEntry project, string map)
    {
        var cache = Msbs.GetValue(project, _ => new ConcurrentDictionary<string, IMsb?>(StringComparer.OrdinalIgnoreCase));
        return cache.GetOrAdd(map, id =>
        {
            var entry = project.Handler.MapData?.PrimaryBank?.Maps?.FirstOrDefault(e => e.Key.Filename.Equals(id, StringComparison.OrdinalIgnoreCase));
            var wrapper = entry?.Value;
            if (wrapper == null)
                return null;

            if (wrapper.MSB is { } loaded)
                return loaded;

            try
            {
                return wrapper.TargetFS.FileExists(wrapper.Path) && MsbGameAdapter.For(project.Descriptor.ProjectType) is { } adapter
                    ? adapter.Read(wrapper.TargetFS.ReadFile(wrapper.Path)!.Value)
                    : null;
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(CutsceneCast), LOC.Get("CUTSCENE_Cast_Msb_Failed", id), e);
                return null;
            }
        });
    }

    /// <summary>
    /// The model masks of an NpcParam row (modelDispMask0-31: 1 shows a mask group), as the Map Editor applies them to
    /// a placed enemy; null with no row or no params. Call on the main thread.
    /// </summary>
    public static int[]? Masks(ProjectEntry project, int npcParamId)
    {
        var bank = project.Handler.ParamData?.PrimaryBank?.Params;
        if (npcParamId <= 0 || bank == null || !bank.TryGetValue("NpcParam", out var npcParam) || !npcParam.ContainsRow(npcParamId))
            return null;

        var row = npcParam[npcParamId]!;
        var masks = new int[32];
        for (var i = 0; i < masks.Length; i++)
        {
            if (row[$"modelDispMask{i}"]?.Value is { } v && Convert.ToBoolean(Convert.ToByte(v)))
                masks[i] = 1;
        }

        return masks;
    }
}

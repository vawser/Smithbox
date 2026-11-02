using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StudioCore.Formats;

/// <summary>
/// An MSB part as the break files see it: Objects for BREAKOBJ, Assets for BREAKGEOM.
/// Key is the ObjectInstanceID for DS2 and the InstanceID for ER and NR; it is unused otherwise.
/// </summary>
public sealed record BreakPart(string Name, string ModelName, int Key);

/// <summary>
/// The counts reported after building the break files of a map.
/// </summary>
public class BreakBuildStats
{
    /// <summary>
    /// Parts whose break data was carried over from the vanilla file of the map.
    /// </summary>
    public int Kept;

    /// <summary>
    /// Parts given break data copied from another instance of their model.
    /// </summary>
    public int Added;
}

/// <summary>
/// The break data of each breakable model, taken from one of its instances in the vanilla files.
/// The piece records are baked for each placement, so the copies only approximate what the game would bake
/// for a new placement, but they have the right number of pieces for the model.
/// </summary>
public class BreakTemplateLibrary
{
    /// <summary>
    /// BREAKOBJ: object model name to its break data.
    /// </summary>
    public readonly Dictionary<string, BreakPieces> ObjectTemplates = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// BREAKOBJ2: object model name to an entry of it.
    /// </summary>
    public readonly Dictionary<string, BREAKOBJ2.Entry> DS2Templates = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// BREAKGEOM: asset model ID to its break data.
    /// </summary>
    public readonly Dictionary<int, BreakPieces> AssetTemplates = new();

    // BREAKGEOM: how many vanilla entries of each asset model are in each LOD file
    private readonly Dictionary<int, int[]> AssetLodCounts = new();

    /// <summary>
    /// The layout of the files read, which new files are written with.
    /// </summary>
    public BREAKOBJ ObjectFormat;

    public void AddBreakObj(IReadOnlyList<BreakPart> objects, BREAKOBJ file)
    {
        ObjectFormat ??= new BREAKOBJ { BigEndian = file.BigEndian, BigEndianHeader = file.BigEndianHeader, Version = file.Version };

        if (objects.Count != file.Objects.Count)
            return;

        for (int i = 0; i < objects.Count; i++)
        {
            if (file.Objects[i] != null)
                ObjectTemplates.TryAdd(objects[i].ModelName, file.Objects[i]);
        }
    }

    public void AddBreakObj2(IReadOnlyList<BreakPart> objects, BREAKOBJ2 file)
    {
        var models = new Dictionary<int, string>();
        foreach (var part in objects)
            models.TryAdd(part.Key, part.ModelName);

        foreach (var entry in file.Entries)
        {
            if (models.TryGetValue((int)entry.ObjectInstanceID, out var model))
                DS2Templates.TryAdd(model, entry);
        }
    }

    public void AddBreakGeom(int lod, BREAKGEOM file)
    {
        foreach (var entry in file.Entries)
        {
            AssetTemplates.TryAdd(entry.ModelID, entry.Pieces);

            if (!AssetLodCounts.TryGetValue(entry.ModelID, out var counts))
                AssetLodCounts[entry.ModelID] = counts = new int[BreakDataBuilder.LodCount];
            counts[lod]++;
        }
    }

    /// <summary>
    /// The LOD file most vanilla entries of an asset model are in.
    /// </summary>
    public int GetAssetLod(int modelId)
    {
        if (!AssetLodCounts.TryGetValue(modelId, out var counts))
            return 0;

        int best = 0;
        for (int lod = 1; lod < counts.Length; lod++)
        {
            if (counts[lod] > counts[best])
                best = lod;
        }
        return best;
    }
}

/// <summary>
/// Builds the break files of a map from its MSB. Parts that are unchanged from the vanilla MSB keep their vanilla
/// break data (or lack of it), so an unedited map builds back to its vanilla files. Other parts get a copy of the
/// break data of their model, if the model is breakable in any vanilla file, and nothing otherwise.
/// </summary>
public static class BreakDataBuilder
{
    /// <summary>
    /// The number of LOD files of a BREAKGEOM map.
    /// </summary>
    public const int LodCount = 3;

    /// <summary>
    /// Gets the BREAKGEOM model ID of an asset model name such as AEG231_021.
    /// </summary>
    public static bool TryGetAssetModelID(string modelName, out int modelId)
    {
        modelId = 0;

        if (modelName == null || modelName.Length != 10 || !modelName.StartsWith("AEG", StringComparison.OrdinalIgnoreCase) || modelName[6] != '_')
            return false;

        if (!int.TryParse(modelName.AsSpan(3, 3), NumberStyles.None, CultureInfo.InvariantCulture, out int category)
            || !int.TryParse(modelName.AsSpan(7, 3), NumberStyles.None, CultureInfo.InvariantCulture, out int id))
            return false;

        modelId = BREAKGEOM.AssetModelIDBase + category * 1000 + id;
        return true;
    }

    /// <summary>
    /// Finds the part of the vanilla MSB that a current part is unchanged from, matched by name and model.
    /// </summary>
    private static Dictionary<string, (BreakPart Part, int Index)> IndexVanilla(IReadOnlyList<BreakPart> vanillaParts)
    {
        var index = new Dictionary<string, (BreakPart, int)>(StringComparer.Ordinal);
        if (vanillaParts == null)
            return index;

        for (int i = 0; i < vanillaParts.Count; i++)
            index.TryAdd(vanillaParts[i].Name, (vanillaParts[i], i));

        return index;
    }

    private static bool IsUnchanged(BreakPart current, BreakPart vanilla, bool compareKey)
    {
        return string.Equals(current.ModelName, vanilla.ModelName, StringComparison.OrdinalIgnoreCase)
            && (!compareKey || current.Key == vanilla.Key);
    }

    /// <summary>
    /// Builds the BREAKOBJ of a map (DeS, DS1, BB, DS3) from its Object parts, in MSB order.
    /// </summary>
    public static BREAKOBJ BuildBreakObj(IReadOnlyList<BreakPart> objects,
        IReadOnlyList<BreakPart> vanillaObjects, BREAKOBJ vanillaFile,
        BreakTemplateLibrary library, BreakBuildStats stats)
    {
        var format = vanillaFile ?? library.ObjectFormat ?? new BREAKOBJ();
        var result = new BREAKOBJ
        {
            BigEndian = format.BigEndian,
            BigEndianHeader = format.BigEndianHeader,
            Version = format.Version
        };

        // The slots of the vanilla file only line up with the vanilla MSB they were made for
        var vanilla = vanillaFile != null && vanillaObjects != null && vanillaObjects.Count == vanillaFile.Objects.Count
            ? IndexVanilla(vanillaObjects)
            : new();

        foreach (var part in objects)
        {
            if (vanilla.TryGetValue(part.Name, out var match) && IsUnchanged(part, match.Part, false))
            {
                var pieces = vanillaFile.Objects[match.Index];
                result.Objects.Add(pieces?.DeepCopy());
                if (pieces != null)
                    stats.Kept++;
            }
            else if (library.ObjectTemplates.TryGetValue(part.ModelName, out var template))
            {
                result.Objects.Add(template.DeepCopy());
                stats.Added++;
            }
            else
            {
                result.Objects.Add(null);
            }
        }

        return result;
    }

    /// <summary>
    /// Builds the BREAKOBJ2 of a DS2 map from its Object parts. Only objects with an ObjectInstanceID can be listed.
    /// </summary>
    public static BREAKOBJ2 BuildBreakObj2(IReadOnlyList<BreakPart> objects,
        IReadOnlyList<BreakPart> vanillaObjects, BREAKOBJ2 vanillaFile,
        BreakTemplateLibrary library, BreakBuildStats stats)
    {
        var result = new BREAKOBJ2();
        var vanilla = IndexVanilla(vanillaObjects);

        var vanillaEntries = new Dictionary<uint, BREAKOBJ2.Entry>();
        var vanillaOwned = new HashSet<uint>();
        if (vanillaFile != null)
        {
            foreach (var entry in vanillaFile.Entries)
                vanillaEntries.TryAdd(entry.ObjectInstanceID, entry);
        }
        // Vanilla files also list entries with an ID of 0, which no object can be matched to
        if (vanillaObjects != null)
        {
            foreach (var part in vanillaObjects)
            {
                if (part.Key != 0)
                    vanillaOwned.Add((uint)part.Key);
            }
        }

        // Entries are written in the order of the vanilla file, then new ones in MSB order
        var built = new Dictionary<uint, BREAKOBJ2.Entry>();
        foreach (var part in objects)
        {
            uint id = (uint)part.Key;
            if (id == 0 || built.ContainsKey(id))
                continue;

            if (vanilla.TryGetValue(part.Name, out var match) && IsUnchanged(part, match.Part, true))
            {
                if (vanillaEntries.TryGetValue(id, out var entry))
                {
                    built[id] = entry.DeepCopy();
                    stats.Kept++;
                }
            }
            else if (library.DS2Templates.TryGetValue(part.ModelName, out var template))
            {
                var entry = template.DeepCopy();
                entry.ObjectInstanceID = id;
                built[id] = entry;
                stats.Added++;
            }
        }

        if (vanillaFile != null)
        {
            foreach (var entry in vanillaFile.Entries)
            {
                if (built.Remove(entry.ObjectInstanceID, out var kept))
                    result.Entries.Add(kept);
                // Entries no vanilla object owned are kept as they were
                else if (!vanillaOwned.Contains(entry.ObjectInstanceID))
                    result.Entries.Add(entry.DeepCopy());
            }
        }

        foreach (var part in objects)
        {
            if (built.Remove((uint)part.Key, out var added))
                result.Entries.Add(added);
        }

        return result;
    }

    /// <summary>
    /// Builds the BREAKGEOM files of an ER or NR map from its Asset parts, one per LOD.
    /// A LOD is null if the map should not have a file for it.
    /// </summary>
    public static BREAKGEOM[] BuildBreakGeom(string mapId, IReadOnlyList<BreakPart> assets,
        IReadOnlyList<BreakPart> vanillaAssets, BREAKGEOM[] vanillaFiles,
        BreakTemplateLibrary library, BreakBuildStats stats)
    {
        var vanilla = IndexVanilla(vanillaAssets);

        var vanillaEntries = new Dictionary<(int, int), (BREAKGEOM.Entry Entry, int Lod)>();
        for (int lod = 0; lod < LodCount; lod++)
        {
            if (vanillaFiles?[lod] == null)
                continue;
            foreach (var entry in vanillaFiles[lod].Entries)
                vanillaEntries.TryAdd((entry.ModelID, entry.InstanceID), (entry, lod));
        }

        var vanillaOwned = new HashSet<(int, int)>();
        if (vanillaAssets != null)
        {
            foreach (var part in vanillaAssets)
            {
                if (TryGetAssetModelID(part.ModelName, out int modelId))
                    vanillaOwned.Add((modelId, part.Key));
            }
        }

        var built = new Dictionary<(int, int), (BREAKGEOM.Entry Entry, int Lod)>();

        // Entries no vanilla asset owned are kept as they were (NR inherits many of these from ER)
        foreach (var (key, value) in vanillaEntries)
        {
            if (!vanillaOwned.Contains(key))
                built[key] = (value.Entry.DeepCopy(), value.Lod);
        }

        foreach (var part in assets)
        {
            if (!TryGetAssetModelID(part.ModelName, out int modelId))
                continue;

            var key = (modelId, part.Key);
            if (built.ContainsKey(key))
                continue;

            if (vanilla.TryGetValue(part.Name, out var match) && IsUnchanged(part, match.Part, true))
            {
                if (vanillaEntries.TryGetValue(key, out var value))
                {
                    built[key] = (value.Entry.DeepCopy(), value.Lod);
                    stats.Kept++;
                }
            }
            else if (library.AssetTemplates.TryGetValue(modelId, out var template))
            {
                var entry = new BREAKGEOM.Entry { ModelID = modelId, InstanceID = part.Key, Pieces = template.DeepCopy() };
                built[key] = (entry, library.GetAssetLod(modelId));
                stats.Added++;
            }
        }

        var files = new BREAKGEOM[LodCount];
        bool isOpenWorld = mapId.StartsWith("m60") || mapId.StartsWith("m61");

        for (int lod = 0; lod < LodCount; lod++)
        {
            var entries = built.Values.Where(v => v.Lod == lod).Select(v => v.Entry)
                .OrderBy(e => e.ModelID).ThenBy(e => e.InstanceID).ToList();

            // Every map has a lod0 file, and open world tiles also have lod1 and lod2
            if (lod > 0 && entries.Count == 0 && !isOpenWorld && vanillaFiles?[lod] == null)
                continue;

            var file = new BREAKGEOM { Entries = entries };
            if (vanillaFiles?[lod] != null)
            {
                file.Version = vanillaFiles[lod].Version;
                file.MapID = (byte[])vanillaFiles[lod].MapID.Clone();
            }
            else
            {
                file.SetMapID(mapId);
            }

            files[lod] = file;
        }

        return files;
    }
}

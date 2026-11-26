using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StudioCore.Editors.MapEditor;

/// <summary>What a variant was made from and what it forked (Model Variants Plan.md §6, V6 storage, used by V5).</summary>
public sealed class AssetVariantRecord
{
    public string Variant { get; set; }
    public string Source { get; set; }
    public DateTime Created { get; set; }

    /// <summary>Forked MATBINs: the source's short name → the variant's.</summary>
    public Dictionary<string, string> Materials { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Forked AET containers: the source's → the variant's.</summary>
    public Dictionary<string, string> Textures { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The colour shift last applied to each forked container (by the variant's container name).</summary>
    public Dictionary<string, AssetVariantShift> Shifts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>A hue / saturation / value shift of a container's colour textures, applied to the source's images.</summary>
public sealed class AssetVariantShift
{
    /// <summary>Degrees, -180 to 180.</summary>
    public float Hue { get; set; }

    /// <summary>Multiplier, 1 = unchanged.</summary>
    public float Saturation { get; set; } = 1;

    /// <summary>Multiplier, 1 = unchanged.</summary>
    public float Value { get; set; } = 1;

    /// <summary>The texture suffixes shifted (after the container name): _a albedo by default.</summary>
    public List<string> Suffixes { get; set; } = new() { "_a" };

    public bool IsIdentity => Math.Abs(Hue) < 0.01f && Math.Abs(Saturation - 1) < 0.001f && Math.Abs(Value - 1) < 0.001f;

    public AssetVariantShift Clone() => new()
    {
        Hue = Hue, Saturation = Saturation, Value = Value, Suffixes = Suffixes.ToList()
    };
}

/// <summary>
/// The project's variant records, kept in &lt;project&gt;/.smithbox/Project/Asset Variants.json. Read on first use,
/// written after each change.
/// </summary>
public static class AssetVariantLineage
{
    private sealed class LineageFile
    {
        public List<AssetVariantRecord> Variants { get; set; } = new();
    }

    private static readonly Dictionary<string, LineageFile> Cache = new(StringComparer.OrdinalIgnoreCase);

    private static string PathOf(ProjectEntry project) =>
        Path.Join(project.Descriptor.ProjectPath, ".smithbox", "Project", "Asset Variants.json");

    private static LineageFile Load(ProjectEntry project)
    {
        var path = PathOf(project);

        if (Cache.TryGetValue(path, out var file))
            return file;

        file = new LineageFile();

        try
        {
            if (System.IO.File.Exists(path))
                file = JsonSerializer.Deserialize<LineageFile>(System.IO.File.ReadAllText(path)) ?? new LineageFile();
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(AssetVariantLineage), LOC.Get("MAP_AssetVariant_Lineage_Read_Failed", path, e.Message), e);
        }

        Cache[path] = file;
        return file;
    }

    private static void Save(ProjectEntry project)
    {
        var path = PathOf(project);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(Load(project), new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(AssetVariantLineage), LOC.Get("MAP_AssetVariant_Lineage_Write_Failed", path, e.Message), e);
        }
    }

    /// <summary>The record of a variant, or null when the asset wasn't made with Make Variant.</summary>
    public static AssetVariantRecord Find(ProjectEntry project, string variant)
    {
        var name = AssetVariant.NormalizeName(variant);
        return name == null ? null : Load(project).Variants.FirstOrDefault(v => string.Equals(v.Variant, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The variants made from an asset.</summary>
    public static List<AssetVariantRecord> VariantsOf(ProjectEntry project, string source)
    {
        var name = AssetVariant.NormalizeName(source);
        return Load(project).Variants.Where(v => string.Equals(v.Source, name, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>Adds (or replaces) a variant's record and writes the file.</summary>
    public static void Add(ProjectEntry project, AssetVariantRecord record)
    {
        var file = Load(project);
        file.Variants.RemoveAll(v => string.Equals(v.Variant, record.Variant, StringComparison.OrdinalIgnoreCase));
        file.Variants.Add(record);
        Save(project);
    }

    /// <summary>Writes a record changed in place (a shift applied).</summary>
    public static void Update(ProjectEntry project) => Save(project);
}

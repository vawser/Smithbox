using SoulsFormats;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Where Material Merge reads the other side from: the materials it offers, what it couldn't read, or why it can't
/// be used at all. <see cref="Error"/> names the localized message (MAT_Merge_Source_{Error}), with
/// <see cref="ErrorDetail"/>. No editor dependencies, so the survey tool can build sources.
/// </summary>
public class MaterialMergeSource
{
    public string Name { get; init; } = "";
    public List<MaterialMergeCandidate> Candidates { get; } = new();
    public List<string> Skipped { get; } = new();

    /// <summary>
    /// Binders the source holds whole, so a vanilla material it lacks there was deleted (another project's own
    /// binders). Empty for a loose file, a JSON export or a loose binder, which may hold only some materials.
    /// </summary>
    public HashSet<string> CompleteBinders { get; } = new(StringComparer.OrdinalIgnoreCase);

    // Files a complete binder holds but couldn't be read: not deleted, just unknown
    private readonly HashSet<(string, string)> Unreadable = new();

    public void AddUnreadable(string binderPath, string key)
    {
        Unreadable.Add((MaterialMergeIndex.NormKey(binderPath), MaterialMergeIndex.NormKey(key)));
        Skipped.Add($"{MaterialMerge.ShortNameOf(key)}: unreadable in {binderPath}");
    }

    public bool IsUnreadable(string binderPath, string key) =>
        Unreadable.Contains((MaterialMergeIndex.NormKey(binderPath), MaterialMergeIndex.NormKey(key)));

    public string Error { get; private set; }
    public string ErrorDetail { get; private set; }

    public bool IsValid => Error == null;

    public static MaterialMergeSource Failed(string name, string error, string detail = null) =>
        new() { Name = name, Error = error, ErrorDetail = detail ?? "" };

    /// <summary>
    /// A loose .mtd or .matbin, or a whole binder of them (.mtdbnd, .matbinbnd, DS2's .bnd), DCX-compressed or not,
    /// told apart by its contents
    /// </summary>
    public static MaterialMergeSource FromMaterialFile(string path)
    {
        byte[] data;
        try
        {
            data = File.ReadAllBytes(path);
        }
        catch (Exception e)
        {
            return Failed(path, "Read_Failed", e.Message);
        }

        try
        {
            if (BND4.Is(data) || BND3.Is(data))
                return FromBinderBytes(data, Path.GetFileName(path), path);
        }
        catch (Exception e)
        {
            return Failed(path, "Read_Failed", e.Message);
        }

        return FromMaterialBytes(data, MaterialMerge.ShortNameOf(path), path);
    }

    /// <summary>
    /// A loose binder. Its files are matched to the project's binder of the same file name
    /// (<see cref="MaterialMergeCandidate.BinderFileName"/>), else by short name. It may hold only some of the
    /// binder's materials, so it never says what was deleted.
    /// </summary>
    public static MaterialMergeSource FromBinderBytes(byte[] data, string binderFileName, string sourceName)
    {
        IBinder binder;
        try
        {
            binder = BND4.Is(data) ? BND4.Read(data) : BND3.Read(data);
        }
        catch (Exception e)
        {
            return Failed(sourceName, "Read_Failed", e.Message);
        }

        var source = new MaterialMergeSource { Name = sourceName };

        foreach (var file in binder.Files)
        {
            var isMatbin = file.Name.Contains(".matbin", StringComparison.OrdinalIgnoreCase);
            var isMtd = !isMatbin && file.Name.Contains(".mtd", StringComparison.OrdinalIgnoreCase);

            if (!isMatbin && !isMtd)
                continue;

            try
            {
                object material = isMatbin ? MATBIN.Read(file.Bytes) : MTD.Read(file.Bytes);

                source.Candidates.Add(new MaterialMergeCandidate(null, file.Name, MaterialMerge.ShortNameOf(file.Name), material)
                {
                    BinderFileName = binderFileName
                });
            }
            catch (Exception)
            {
                source.Skipped.Add($"{MaterialMerge.ShortNameOf(file.Name)}: unreadable in {binderFileName}");
            }
        }

        if (source.Candidates.Count == 0 && source.Skipped.Count == 0)
            return Failed(sourceName, "Not_Material");

        return source;
    }

    public static MaterialMergeSource FromMaterialBytes(byte[] data, string shortName, string sourceName)
    {
        object material;
        try
        {
            material = MATBIN.Is(data) ? MATBIN.Read(data)
                : MTD.Is(data) ? MTD.Read(data)
                : null;
        }
        catch (Exception e)
        {
            return Failed(sourceName, "Read_Failed", e.Message);
        }

        if (material == null)
            return Failed(sourceName, "Not_Material");

        var source = new MaterialMergeSource { Name = sourceName };
        source.Candidates.Add(new MaterialMergeCandidate(null, null, shortName, material));
        return source;
    }

    /// <summary>
    /// A Material JSON export (Export, Copy as JSON). Named by the name it records, else by the file's name.
    /// </summary>
    public static MaterialMergeSource FromJsonFile(string path)
    {
        string text;
        try
        {
            text = File.ReadAllText(path);
        }
        catch (Exception e)
        {
            return Failed(path, "Read_Failed", e.Message);
        }

        return FromJsonText(text, MaterialMerge.ShortNameOf(path), path);
    }

    public static MaterialMergeSource FromJsonText(string text, string fallbackName, string sourceName)
    {
        var json = MaterialJson.FromText(text);

        if (json == null)
        {
            var set = MaterialJson.SetJson.FromText(text);
            return set == null ? Failed(sourceName, "Not_Json") : FromSet(set, sourceName);
        }

        var material = json.ToMaterial(out var error);
        if (material == null)
            return Failed(sourceName, "Bad_Param", error);

        // Not trimmed: a few vanilla names end in a space (DS3 c2170[Veil] and others)
        var name = string.IsNullOrWhiteSpace(json.Name) ? fallbackName : json.Name;

        var source = new MaterialMergeSource { Name = sourceName };
        source.Candidates.Add(new MaterialMergeCandidate(null, null, name, material));
        return source;
    }

    /// <summary>
    /// A material set (Export Modified as JSON): each material where its entry says, else by short name. A material
    /// that doesn't read is skipped, not the whole set. A set holds only some materials, so it never says what was
    /// deleted.
    /// </summary>
    private static MaterialMergeSource FromSet(MaterialJson.SetJson set, string sourceName)
    {
        var source = new MaterialMergeSource { Name = sourceName };

        foreach (var entry in set.Materials)
        {
            var json = entry?.Material;
            var key = string.IsNullOrWhiteSpace(entry?.Key) ? null : entry.Key;
            var name = !string.IsNullOrWhiteSpace(json?.Name) ? json.Name : MaterialMerge.ShortNameOf(key);

            if (json == null || json.Kind is not ("MTD" or "MATBIN"))
            {
                source.Skipped.Add($"{name}: not a material");
                continue;
            }

            var material = json.ToMaterial(out var error);
            if (material == null)
            {
                source.Skipped.Add($"{name}: param {error} doesn't fit its type");
                continue;
            }

            var binder = string.IsNullOrWhiteSpace(entry.Binder) || key == null ? null : entry.Binder;
            source.Candidates.Add(new MaterialMergeCandidate(binder, binder == null ? null : key, name, material));
        }

        if (source.Candidates.Count == 0 && source.Skipped.Count == 0)
            return Failed(sourceName, "Not_Json");

        return source;
    }
}

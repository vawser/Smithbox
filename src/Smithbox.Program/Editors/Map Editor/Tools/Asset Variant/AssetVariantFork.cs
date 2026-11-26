using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>Where a game keeps its asset files (Claude/Documentation/Model Variants Plan.md §6.1).</summary>
public enum AssetVariantLayout
{
    /// <summary>ER, NR: asset/aeg/aegXXX/aegXXX_YYY.*</summary>
    Aeg,

    /// <summary>AC6: asset/environment/geometry/aegXXX_YYY.*, with loose .geomhkx.dcx files beside the binders</summary>
    Ac6Environment
}

/// <summary>
/// The byte level part of making an asset variant: which files an asset has, and each file renamed to the variant's
/// name. Only SoulsFormats is used (and <see cref="MapPieceRename.ReplaceStem" />), so Claude/Tools/AssetVariantSurvey
/// compiles this file to check it against every vanilla asset.
/// <para>
/// Asset names have a fixed width (aegXXX_YYY), so the old name is replaced byte for byte where it appears whole:
/// binder entry names, the FLVER's root node and bones, Havok body names. Every offset stays valid and nothing has to
/// be parsed and rewritten.
/// </para>
/// </summary>
public static class AssetVariantFork
{
    /// <summary>The files an asset can have, as suffixes of its name. Only the model binder is always present.</summary>
    public static readonly string[] FileSuffixes =
    [
        ".geombnd.dcx",
        "_h.geomhkxbnd.dcx",
        "_l.geomhkxbnd.dcx",
        "_h.geomhkx.dcx",
        "_l.geomhkx.dcx"
    ];

    private static readonly Regex AssetName = new(@"^aeg(\d{3})_(\d{3})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Whether a name is an asset name, aegXXX_YYY.</summary>
    public static bool IsAssetName(string name) => name != null && AssetName.IsMatch(name);

    /// <summary>The AEG group of an asset name (027 of aeg027_053), or -1.</summary>
    public static int Group(string name)
    {
        var m = AssetName.Match(name ?? "");
        return m.Success ? int.Parse(m.Groups[1].Value) : -1;
    }

    /// <summary>The number within its group (053 of aeg027_053), or -1.</summary>
    public static int Number(string name)
    {
        var m = AssetName.Match(name ?? "");
        return m.Success ? int.Parse(m.Groups[2].Value) : -1;
    }

    /// <summary>An asset name from its group and number.</summary>
    public static string MakeName(int group, int number) => $"aeg{group:000}_{number:000}";

    /// <summary>The asset's AssetEnvironmentGeometryParam row: group * 1000 + number (aeg027_053 → 27053).</summary>
    public static int ParamRowId(string name) => Group(name) * 1000 + Number(name);

    /// <summary>The folder of an asset's files, with a trailing slash.</summary>
    public static string Folder(AssetVariantLayout layout, string name)
    {
        var lower = name.ToLowerInvariant();

        return layout switch
        {
            AssetVariantLayout.Ac6Environment => "/asset/environment/geometry/",
            _ => $"/asset/aeg/{lower[..6]}/"
        };
    }

    /// <summary>The path of one of an asset's files.</summary>
    public static string PathOf(AssetVariantLayout layout, string name, string suffix)
    {
        return Folder(layout, name) + name.ToLowerInvariant() + suffix;
    }

    /// <summary>
    /// The asset's files that exist (by <paramref name="exists" />), each with the path of the variant's copy.
    /// </summary>
    public static List<(string Source, string Target)> Files(AssetVariantLayout layout, string sourceName, string targetName,
        Func<string, bool> exists)
    {
        var result = new List<(string, string)>();

        foreach (var suffix in FileSuffixes)
        {
            var source = PathOf(layout, sourceName, suffix);

            if (exists(source))
                result.Add((source, PathOf(layout, targetName, suffix)));
        }

        return result;
    }

    /// <summary>
    /// A file of the asset renamed: a binder's entry names take the new name, and every file (also those compressed
    /// inside the binder, and a loose DCX file) has the old name replaced where it appears whole, keeping the letter
    /// case of each match. The DCX of the file and of each inner file is kept.
    /// </summary>
    public static byte[] Rename(Memory<byte> data, string oldName, string newName)
    {
        if (oldName.Length != newName.Length)
            throw new ArgumentException($"{oldName} and {newName} must be the same length");

        var type = DCX.Type.None;
        var raw = data;

        if (DCX.Is(raw))
            raw = DCX.Decompress(raw, out type);

        var magic = raw.Length >= 4 ? Encoding.ASCII.GetString(raw.Span[..4]) : "";

        if (magic is "BND3" or "BND4")
        {
            // Read from the original bytes, so the binder writes back with the same DCX
            IBinder binder = magic == "BND3" ? BND3.Read(data) : BND4.Read(data);

            foreach (var file in binder.Files)
            {
                file.Name = ReplaceKeepingCase(file.Name, oldName, newName);
                file.Bytes = RenameInner(file.Bytes, oldName, newName);
            }

            return binder is BND3 b3 ? b3.Write() : ((BND4)binder).Write();
        }

        var patched = RenameFile(raw, oldName, newName);
        return type == DCX.Type.None ? patched.ToArray() : DCX.Compress(patched.Span, type);
    }

    /// <summary>An inner file renamed, decompressed and compressed again when it is a DCX.</summary>
    private static Memory<byte> RenameInner(Memory<byte> data, string oldName, string newName)
    {
        if (!DCX.Is(data))
            return RenameFile(data, oldName, newName);

        var raw = DCX.Decompress(data, out var type);
        var patched = RenameFile(raw, oldName, newName);

        // Untouched: keep the original compressed bytes
        return patched.Equals(raw) ? data : DCX.Compress(patched.Span, type);
    }

    #region Textures (V4)
    private static readonly Regex AetName = new(@"^aet(\d{3})_(\d{3})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// The AET container a texture path names: the first 10 characters of its file name (AET000_143_a.tif →
    /// aet000_143), as the game and the renderer find it; null for any other texture (SYSTEX_*, character textures…).
    /// </summary>
    public static string ContainerOf(string texturePath)
    {
        if (string.IsNullOrEmpty(texturePath))
            return null;

        var cut = Math.Max(texturePath.LastIndexOf('\\'), texturePath.LastIndexOf('/'));
        var m = AetName.Match(texturePath[(cut + 1)..]);
        return m.Success ? m.Value.ToLowerInvariant() : null;
    }

    /// <summary>An AET container name from its group and number.</summary>
    public static string MakeContainer(int group, int number) => $"aet{group:000}_{number:000}";

    public static int ContainerGroup(string container) => int.Parse(container[3..6]);
    public static int ContainerNumber(string container) => int.Parse(container[7..10]);

    /// <summary>The path of an AET container's TPF, or of its low-detail twin (_l).</summary>
    public static string AetPath(AssetVariantLayout layout, string container, bool low)
    {
        var name = container.ToLowerInvariant() + (low ? "_l" : "") + ".tpf.dcx";

        return layout == AssetVariantLayout.Ac6Environment
            ? "/asset/environment/texture/" + name
            : $"/asset/aet/{name[..6]}/{name}";
    }

    /// <summary>
    /// An AET TPF with its textures renamed to another container (AET000_143_a → AET000_901_a; the _l twin's
    /// AET000_143_a_l → AET000_901_a_l), keeping the letter case, the texture data and the DCX.
    /// </summary>
    public static byte[] RenameTpf(Memory<byte> data, string oldContainer, string newContainer)
    {
        var tpf = TPF.Read(data);

        foreach (var texture in tpf.Textures)
            texture.Name = ReplaceKeepingCase(texture.Name, oldContainer, newContainer);

        return tpf.Write();
    }

    /// <summary>
    /// A texture path pointed at another container: the file name's container replaced and, when the group changes,
    /// a folder named after the old group (ER's …\texture\AET000\AET000_143_a.tif) renamed with it. Paths naming
    /// another container are returned as they are.
    /// </summary>
    public static string RetargetTexturePath(string path, IReadOnlyDictionary<string, string> containers)
    {
        var container = ContainerOf(path);

        if (container == null || !containers.TryGetValue(container, out var target))
            return path;

        var result = ReplaceKeepingCase(path, container, target);
        var oldGroup = container[..6];
        var newGroup = target[..6];

        if (!string.Equals(oldGroup, newGroup, StringComparison.OrdinalIgnoreCase))
        {
            foreach (var separator in new[] { '\\', '/' })
                result = ReplaceKeepingCase(result, separator + oldGroup + separator, separator + newGroup + separator);
        }

        return result;
    }
    #endregion

    /// <summary>The short name of a FLVER material path (N:\...\matxml\AEG001_379_Ice.matxml → AEG001_379_Ice).</summary>
    public static string MaterialShortName(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "";

        var cut = Math.Max(path.LastIndexOf('\\'), path.LastIndexOf('/'));
        var file = path[(cut + 1)..];
        var dot = file.IndexOf('.');
        return dot >= 0 ? file[..dot] : file;
    }

    /// <summary>The material short names of the FLVERs in an asset's model binder, in first-use order.</summary>
    public static List<string> MaterialShortNames(Memory<byte> geombnd)
    {
        var names = new List<string>();

        foreach (var file in BND4.Read(geombnd).Files.Where(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)))
        {
            foreach (var material in FLVER2.Read(file.Bytes).Materials)
            {
                var name = MaterialShortName(material.MTD);
                if (name.Length > 0 && !names.Contains(name, StringComparer.OrdinalIgnoreCase))
                    names.Add(name);
            }
        }

        return names;
    }

    /// <summary>
    /// A model binder with its FLVERs' materials pointed at other MATBINs (old short name → new), keeping each path's
    /// folder and extension, and those materials' texture slots pointed at forked AET containers (old → new; may be
    /// empty). The FLVERs are rewritten (the names change length); the other entries and the DCX are kept.
    /// </summary>
    public static byte[] RetargetMaterials(Memory<byte> geombnd, IReadOnlyDictionary<string, string> shortNames,
        IReadOnlyDictionary<string, string> containers = null)
    {
        var binder = BND4.Read(geombnd);

        foreach (var file in binder.Files.Where(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)))
        {
            var flver = FLVER2.Read(file.Bytes);
            var changed = false;

            foreach (var material in flver.Materials)
            {
                var path = material.MTD ?? "";
                var old = MaterialShortName(path);
                var match = shortNames.Keys.FirstOrDefault(k => string.Equals(k, old, StringComparison.OrdinalIgnoreCase));

                if (match == null)
                    continue;

                var cut = Math.Max(path.LastIndexOf('\\'), path.LastIndexOf('/')) + 1;
                material.MTD = path[..cut] + shortNames[match] + path[(cut + old.Length)..];
                changed = true;

                // Only a forked material's slots: a shared one keeps naming the source's textures
                if (containers?.Count > 0)
                {
                    foreach (var texture in material.Textures)
                        texture.Path = RetargetTexturePath(texture.Path, containers);
                }
            }

            if (changed)
                file.Bytes = flver.Write();
        }

        return binder.Write();
    }

    /// <summary>
    /// The FLVER strings a rename leaves alone: material paths (MATBIN names) and texture paths. They name files of
    /// their own, which the variant shares with its source until a material or texture fork retargets them.
    /// </summary>
    public static IEnumerable<string> KeptPaths(FLVER2 flver)
    {
        return flver.Materials
            .SelectMany(m => m.Textures.Select(t => t.Path).Prepend(m.MTD))
            .Where(path => !string.IsNullOrEmpty(path));
    }

    /// <summary>
    /// One (uncompressed) file renamed. A FLVER keeps its material and texture paths (<see cref="KeptPaths" />): many
    /// assets name their MATBINs after themselves (AEG001_379_Ice.matxml, M[AEG065_650]_body_01.matxml), AC6 some
    /// textures (SLOD_AEG065_650_a.tif), and a renamed path would name a file that doesn't exist.
    /// </summary>
    private static Memory<byte> RenameFile(Memory<byte> raw, string oldName, string newName)
    {
        // Every occurrence, not only whole words: asset names have a fixed width (see ReplaceStem's wholeWord)
        var patched = MapPieceRename.ReplaceStem(raw, oldName, newName, wholeWord: false);

        if (patched.Equals(raw) || raw.Length < 6 || !raw.Span[..6].SequenceEqual("FLVER\0"u8))
            return patched;

        // Every ER, NR and AC6 asset FLVER is a FLVER2
        var kept = KeptPaths(FLVER2.Read(raw))
            .Where(path => path.Contains(oldName, StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToList();

        if (kept.Count == 0)
            return patched;

        var original = raw.Span;
        var output = patched.Span;

        foreach (var path in kept)
        {
            foreach (var encoded in new[] { Encoding.Unicode.GetBytes(path), Encoding.ASCII.GetBytes(path) })
            {
                for (var at = original.IndexOf(encoded); at >= 0;)
                {
                    original.Slice(at, encoded.Length).CopyTo(output.Slice(at, encoded.Length));

                    var next = original[(at + encoded.Length)..].IndexOf(encoded);
                    at = next < 0 ? -1 : at + encoded.Length + next;
                }
            }
        }

        return patched;
    }

    /// <summary>Every match of <paramref name="oldValue" /> replaced, each keeping the letter case it had.</summary>
    public static string ReplaceKeepingCase(string text, string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(text) || oldValue.Length != newValue.Length)
            return text;

        var builder = new StringBuilder(text);
        var at = 0;

        while ((at = text.IndexOf(oldValue, at, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            for (var c = 0; c < newValue.Length; c++)
            {
                var old = text[at + c];
                builder[at + c] = char.IsUpper(old) ? char.ToUpperInvariant(newValue[c]) : char.ToLowerInvariant(newValue[c]);
            }

            at += oldValue.Length;
        }

        return builder.ToString();
    }

    /// <summary>
    /// How often a name appears in the data, whole or not, as single bytes or UTF-16, ignoring case (decompressed
    /// first when the data is a DCX). Used to check that nothing of the old name is left.
    /// </summary>
    public static int CountName(Memory<byte> data, string name)
    {
        if (DCX.Is(data))
            data = DCX.Decompress(data, out _);

        var span = data.Span;
        var count = 0;

        foreach (var wide in new[] { true, false })
        {
            var width = wide ? 2 : 1;
            var length = name.Length * width;

            for (var at = 0; at + length <= span.Length; at++)
            {
                var match = true;

                for (var c = 0; c < name.Length && match; c++)
                {
                    int value = wide ? span[at + c * 2] | (span[at + c * 2 + 1] << 8) : span[at + c];
                    match = value <= 0x7F && char.ToLowerInvariant((char)value) == char.ToLowerInvariant(name[c]);
                }

                if (match)
                    count++;
            }
        }

        return count;
    }
}

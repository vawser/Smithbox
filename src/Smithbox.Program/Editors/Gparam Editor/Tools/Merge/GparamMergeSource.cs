using Andre.IO.VFS;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Where Gparam Merge reads the other side from. Saved as an int in <c>GparamEditor_Merge_Source_Type</c> (P4).
/// </summary>
public enum GparamMergeSourceType
{
    Project = 0,
    ModFolder = 1,
    LooseFile = 2,
    TransferExport = 3
}

/// <summary>
/// A file as the locator names it: <c>Filename</c> without extensions, <c>Extension</c> without ".dcx"
/// (gparam, gparambnd, fltparam). Compared ignoring case. A null extension (a transfer export, which records only the
/// file name) matches any extension.
/// </summary>
public sealed record GparamMergeFileKey(string Filename, string Extension)
{
    public bool Equals(GparamMergeFileKey other) =>
        other != null &&
        string.Equals(Filename, other.Filename, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(Extension, other.Extension, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() =>
        HashCode.Combine(Filename?.ToLowerInvariant(), Extension?.ToLowerInvariant());

    public bool Matches(GparamMergeFileKey other) =>
        other != null &&
        string.Equals(Filename, other.Filename, StringComparison.OrdinalIgnoreCase) &&
        (Extension == null || other.Extension == null ||
         string.Equals(Extension, other.Extension, StringComparison.OrdinalIgnoreCase));

    /// <summary>The key of a path, as ProjectFileLocator derives it (".dcx" dropped, then the last extension)</summary>
    public static GparamMergeFileKey FromPath(string path)
    {
        var name = Path.GetFileName(path.Replace('\\', '/'));
        var ext = Path.GetExtension(name).TrimStart('.').ToLowerInvariant();
        var stem = Path.GetFileNameWithoutExtension(name);

        if (ext == "dcx")
        {
            var prev = Path.GetExtension(stem);
            if (!string.IsNullOrEmpty(prev))
            {
                ext = prev.TrimStart('.').ToLowerInvariant();
                stem = Path.GetFileNameWithoutExtension(stem);
            }
        }

        return new GparamMergeFileKey(stem, ext);
    }

    public override string ToString() => Extension == null ? Filename : $"{Filename}.{Extension}";
}

/// <summary>
/// Something the merge couldn't use. <see cref="Code"/> names the localized message (GPARAM_Merge_Problem_{Code},
/// P4); <see cref="Subject"/> is the file or item, <see cref="Detail"/> any extra text (an exception message).
/// </summary>
public sealed record GparamMergeProblem(string Code, string Subject, string Detail = "")
{
    public override string ToString() => $"{Code}: {Subject}{(string.IsNullOrEmpty(Detail) ? "" : $" ({Detail})")}";
}

/// <summary>One GPARAM a source offers</summary>
public class GparamMergeSourceFile
{
    public GparamMergeFileKey Key { get; init; }

    /// <summary>Where it was read from, for display</summary>
    public string Path { get; init; }

    /// <summary>
    /// The source holds this file whole, so vanilla entries it lacks were deleted (Removed by Them). False for a
    /// transfer export of groups, fields or values.
    /// </summary>
    public bool Whole { get; init; } = true;

    /// <summary>Reads the GPARAM (on the worker). Null with a problem when it can't.</summary>
    public Func<(GPARAM Gparam, GparamMergeProblem Problem)> Read { get; init; }

    /// <summary>
    /// The path inside a game or mod folder ("/param/drawparam/…"), where Add File writes it in this project
    /// (D2). Null when the source doesn't say where it goes (a loose file, a transfer export).
    /// </summary>
    public string RelativePath { get; init; }

    /// <summary>The file's bytes as stored (DCX and binder kept), for Add File. Null when <see cref="RelativePath"/> is.</summary>
    public Func<byte[]> ReadBytes { get; init; }
}

/// <summary>
/// The other side of a merge: the GPARAMs it offers, or why it can't be used (<see cref="Error"/>, a problem code).
/// No editor dependencies (Claude/Documentation/Gparam Merge Plan.md §3); the editor builds Project and Transfer
/// sources through <c>GparamMergeSources</c>.
/// </summary>
public class GparamMergeSource : IDisposable
{
    public GparamMergeSourceType Type { get; init; }
    public string Name { get; init; } = "";

    /// <summary>A file system the source opened itself (a mod folder), disposed with it</summary>
    private VirtualFileSystem OwnedFileSystem { get; init; }

    public void Dispose() => OwnedFileSystem?.Dispose();

    public List<GparamMergeSourceFile> Files { get; } = new();

    /// <summary>Found but not offered (unreadable containers), for the report</summary>
    public List<GparamMergeProblem> Problems { get; } = new();

    public GparamMergeProblem Error { get; private set; }
    public bool IsValid => Error == null;

    public static GparamMergeSource Failed(GparamMergeSourceType type, string name, string code, string detail = "") =>
        new() { Type = type, Name = name, Error = new GparamMergeProblem(code, name, detail) };

    #region File system

    /// <summary>
    /// Every GPARAM path a file system holds, by the locator's rules for any game: /param/**.gparam(.dcx),
    /// /param/**.gparambnd(.dcx), /filter/**.fltparam(.dcx). Paths start with '/'.
    /// </summary>
    public static List<string> FindGparamPaths(VirtualFileSystem fs)
    {
        var paths = new List<string>();
        paths.AddRange(Find(fs, "/param", @"(?i)\.gparam(\.dcx)?$"));
        paths.AddRange(Find(fs, "/param", @"(?i)\.gparambnd(\.dcx)?$"));
        paths.AddRange(Find(fs, "/filter", @"(?i)\.fltparam(\.dcx)?$"));

        return paths
            .Select(p => p.Replace('\\', '/'))
            .Select(p => p.StartsWith('/') ? p : "/" + p)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IEnumerable<string> Find(VirtualFileSystem fs, string dir, string pattern)
    {
        try
        {
            return fs.DirectoryExists(dir) ? fs.GetFileNamesMatchingRecursive(dir, pattern).ToList() : new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// The GPARAMs of a file system (another project's folder, a mod folder). Each is whole. A key found twice (a
    /// .gparam beside a .gparam.dcx, or an unpacked binder folder beside the file) keeps the shallowest path (then
    /// the first by name) and reports the others. With
    /// <paramref name="ownsFileSystem"/>, disposing the source disposes <paramref name="fs"/>.
    /// </summary>
    public static GparamMergeSource FromFileSystem(GparamMergeSourceType type, string name, VirtualFileSystem fs,
        bool ownsFileSystem = false)
    {
        var paths = FindGparamPaths(fs);

        if (paths.Count == 0)
        {
            if (ownsFileSystem)
                fs.Dispose();

            return Failed(type, name, "No_Gparams");
        }

        var source = new GparamMergeSource { Type = type, Name = name, OwnedFileSystem = ownsFileSystem ? fs : null };
        var seen = new HashSet<GparamMergeFileKey>();

        // The file in place wins over a copy in a sub folder (an unpacked binder such as m21_00_0000-gparambnd-dcx/)
        foreach (var path in paths.OrderBy(p => p.Count(c => c == '/')).ThenBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            var key = GparamMergeFileKey.FromPath(path);

            if (!seen.Add(key))
            {
                source.Problems.Add(new GparamMergeProblem("Duplicate_File", path));
                continue;
            }

            source.Files.Add(new GparamMergeSourceFile
            {
                Key = key,
                Path = path,
                RelativePath = path,
                ReadBytes = () => fs.ReadFile(path)?.ToArray(),
                Read = () =>
                {
                    try
                    {
                        return ReadGparam(fs.ReadFile(path)?.ToArray(), key, path);
                    }
                    catch (Exception e)
                    {
                        return (null, new GparamMergeProblem("Read_Failed", path, e.Message));
                    }
                }
            });
        }

        return source;
    }

    /// <summary>
    /// The folder to read a mod from: the chosen folder, its parent or its grandparent (so the mod root, its param
    /// folder or a drawparam folder all work), the first holding any of <paramref name="expectedPaths"/> (this
    /// project's GPARAM paths), else the first holding any GPARAM. Null when none does.
    /// </summary>
    public static string ResolveModRoot(string chosen, IEnumerable<string> expectedPaths)
    {
        if (string.IsNullOrEmpty(chosen) || !Directory.Exists(chosen))
            return null;

        var candidates = new List<string>();
        for (var dir = new DirectoryInfo(chosen); dir != null && candidates.Count < 3; dir = dir.Parent)
            candidates.Add(dir.FullName);

        var expected = expectedPaths.Select(p => p.TrimStart('/')).ToList();

        foreach (var candidate in candidates)
        {
            if (expected.Any(p => File.Exists(Path.Combine(candidate, p))))
                return candidate;
        }

        foreach (var candidate in candidates)
        {
            using var fs = new RealVirtualFileSystem(candidate, true);
            if (FindGparamPaths(fs).Count > 0)
                return candidate;
        }

        return null;
    }

    #endregion

    #region Loose file

    /// <summary>
    /// One loose .gparam(.dcx) / .fltparam(.dcx) / .gparambnd(.dcx), matched by its file name. Whole.
    /// </summary>
    public static GparamMergeSource FromLooseFile(string path)
    {
        if (!File.Exists(path))
            return Failed(GparamMergeSourceType.LooseFile, path, "Not_Found");

        var key = GparamMergeFileKey.FromPath(path);

        if (key.Extension is not ("gparam" or "gparambnd" or "fltparam"))
            return Failed(GparamMergeSourceType.LooseFile, path, "Not_Gparam");

        var source = new GparamMergeSource { Type = GparamMergeSourceType.LooseFile, Name = path };

        source.Files.Add(new GparamMergeSourceFile
        {
            Key = key,
            Path = path,
            Read = () =>
            {
                try
                {
                    return ReadGparam(File.ReadAllBytes(path), key, path);
                }
                catch (Exception e)
                {
                    return (null, new GparamMergeProblem("Read_Failed", path, e.Message));
                }
            }
        });

        return source;
    }

    #endregion

    #region Given GPARAMs (transfer exports)

    /// <summary>A GPARAM already read (a whole-GPARAM transfer export), matched by <paramref name="key"/></summary>
    public static GparamMergeSource FromGparam(GparamMergeSourceType type, string name, GparamMergeFileKey key,
        GPARAM gparam, bool whole)
    {
        var source = new GparamMergeSource { Type = type, Name = name };

        source.Files.Add(new GparamMergeSourceFile
        {
            Key = key,
            Path = name,
            Whole = whole,
            Read = () => (gparam, null)
        });

        return source;
    }

    /// <summary>
    /// Their side of a partial export: a copy of <paramref name="mine"/> with the exported groups put in (each
    /// replaces the group of its slot key, else is added). Merged against vanilla, entries the export leaves as the
    /// game has them keep mine, and nothing it lacks is removed (not whole).
    /// </summary>
    public static GPARAM OverlayGroups(GPARAM mine, IEnumerable<GPARAM.Param> groups)
    {
        var theirs = mine.Clone();

        foreach (var group in groups.Where(g => g != null))
        {
            var clone = GparamMerge.CloneGroup(group);
            var current = GparamMerge.FindGroup(theirs, group.Key);

            if (current != null)
                theirs.Params[theirs.Params.IndexOf(current)] = clone;
            else
                theirs.Params.Add(clone);
        }

        return theirs;
    }

    /// <summary>As <see cref="OverlayGroups"/>, for exported fields put into one group (null problem on success)</summary>
    public static GPARAM OverlayFields(GPARAM mine, string groupKey, IEnumerable<GPARAM.IField> fields,
        out GparamMergeProblem problem)
    {
        problem = null;
        var theirs = mine.Clone();
        var group = GparamMerge.FindGroup(theirs, groupKey);

        if (group == null)
        {
            problem = new GparamMergeProblem("Group_Missing", groupKey ?? "");
            return null;
        }

        foreach (var field in fields.Where(f => f != null))
        {
            var clone = group.CloneField(field);
            clone.SyncCapacity();
            var current = GparamMerge.FindField(group, field.Key);

            if (current != null)
                group.Fields[group.Fields.IndexOf(current)] = clone;
            else
                group.Fields.Add(clone);
        }

        return theirs;
    }

    /// <summary>
    /// As <see cref="OverlayGroups"/>, for exported values put into one field: each exported ID's values replace all
    /// of that ID's keyframes (where mine's first one was, else at the end). Null with a problem when the group or
    /// field is missing or the values are of another type.
    /// </summary>
    public static GPARAM OverlayValues(GPARAM mine, string groupKey, string fieldKey, IReadOnlyList<GPARAM.IFieldValue> values,
        out GparamMergeProblem problem)
    {
        problem = null;
        var theirs = mine.Clone();
        var field = GparamMerge.FindField(GparamMerge.FindGroup(theirs, groupKey), fieldKey);

        if (field == null)
        {
            problem = new GparamMergeProblem("Field_Missing", $"{groupKey} -> {fieldKey}");
            return null;
        }

        if (values.Any(v => v == null || !field.AcceptsValue(v)))
        {
            problem = new GparamMergeProblem("Value_Type_Mismatch", $"{groupKey} -> {fieldKey}");
            return null;
        }

        foreach (var ids in values.GroupBy(v => v.ID))
        {
            var current = GparamMerge.KeyframesOf(field, ids.Key);
            var index = current.Count > 0 ? field.IndexOfValue(current[0]) : field.Values.Count;

            foreach (var value in current)
                field.RemoveValue(value);

            foreach (var value in ids)
                field.InsertValueAt(index++, CloneValue(value));
        }

        return theirs;
    }

    /// <summary>A copy of a free-standing value (not in a field)</summary>
    private static GPARAM.IFieldValue CloneValue(GPARAM.IFieldValue value)
    {
        var clone = (GPARAM.IFieldValue)Activator.CreateInstance(value.GetType());
        clone.ID = value.ID;
        clone.TimeOfDay = value.TimeOfDay;
        clone.Value = value.Value;
        return clone;
    }

    #endregion

    #region Reading

    /// <summary>
    /// A GPARAM from file bytes (DCX or not). A binder is searched for the file whose name (without its last
    /// extension) is the key's file name, as GparamBank reads it.
    /// </summary>
    public static (GPARAM, GparamMergeProblem) ReadGparam(byte[] data, GparamMergeFileKey key, string path)
    {
        if (data == null)
            return (null, new GparamMergeProblem("Read_Failed", path));

        try
        {
            var plain = DCX.Is(data) ? DCX.Decompress(data).ToArray() : data;

            if (BND4.Is(plain))
            {
                var binder = BND4.Read(plain);
                // As GparamBank (name without its last extension), or as a key (".dcx" dropped too)
                var file = binder.Files.FirstOrDefault(e =>
                    string.Equals(Path.GetFileNameWithoutExtension(e.Name), key.Filename, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(GparamMergeFileKey.FromPath(e.Name ?? "").Filename, key.Filename, StringComparison.OrdinalIgnoreCase));

                if (file == null)
                    return (null, new GparamMergeProblem("Not_In_Binder", path, key.Filename));

                return (GPARAM.Read(file.Bytes), null);
            }

            return (GPARAM.Read(plain), null);
        }
        catch (Exception e)
        {
            return (null, new GparamMergeProblem("Read_Failed", path, e.Message));
        }
    }

    #endregion
}

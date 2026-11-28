using Andre.IO.VFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>Where a merge's other side comes from. Saved as an int in <c>TimeActEditor_Merge_Source_Type</c> (P5).</summary>
public enum TimeActMergeSourceType
{
    /// <summary>Another project of the same game: the anibnds in its own folder</summary>
    Project = 0,

    /// <summary>A mod's folder: the anibnds it holds at this project's paths</summary>
    ModFolder = 1,

    /// <summary>One anibnd or container, matched by file name; or one .tae, merged into the selected source</summary>
    LooseFile = 2
}

/// <summary>
/// How sources are matched: the path as the locator has it, without the compression extension, case ignored
/// ("/chr/c3000.anibnd.dcx" and "/chr/c3000.anibnd" are one source)
/// </summary>
public static class TimeActMergeSourceKey
{
    public static string Of(string path)
    {
        var key = (path ?? "").Replace('\\', '/').ToLowerInvariant();

        if (!key.StartsWith('/'))
            key = "/" + key;

        return key.EndsWith(".dcx") ? key[..^4] : key;
    }

    /// <summary>The file name part of a key, for matching a loose file</summary>
    public static string FileName(string key) => key[(key.LastIndexOf('/') + 1)..];
}

/// <summary>
/// One source file the other side has. Its time acts are read on the worker through <see cref="Read"/>, which the
/// editor builds (it reads them the way this project reads its own).
/// </summary>
public class TimeActMergeSourceFile
{
    /// <summary><see cref="TimeActMergeSourceKey"/> of its path; a loose file's is only its file name</summary>
    public string Key { get; init; }

    /// <summary>Its path in the source (relative to the source's root), for display and for Add Source (P4)</summary>
    public string Path { get; init; }

    /// <summary>The source holds it whole, so anything vanilla it lacks was deleted</summary>
    public bool Whole { get; init; } = true;

    /// <summary>A loose file matched by file name alone, not by its path</summary>
    public bool ByFileName { get; init; }

    /// <summary>
    /// A lone .tae: only the time act of this binder ID is merged (the source holds that one whole and says nothing
    /// of the rest). Null for a whole source file.
    /// </summary>
    public int? OnlyBinderID { get; init; }

    /// <summary>For a lone .tae: the target it goes into (the selected source), as the editor's key</summary>
    public string TargetKey { get; init; }

    public Func<(List<TimeActMergeTimeAct> TimeActs, TimeActMergeProblem Problem)> Read { get; init; }

    /// <summary>The editor's reader for it (a TimeActContainerWrapper over the source's file system), for its clips</summary>
    public object Tag { get; init; }

    public override string ToString() => Path;
}

/// <summary>
/// The other side of a merge: its files and the problems found reading it. Owns the file system of a mod folder or
/// loose file, so dispose it once the merge is done with it (Add Source reads from it).
/// </summary>
public class TimeActMergeSource : IDisposable
{
    public TimeActMergeSourceType Type { get; init; }

    /// <summary>The project's name, the folder or the file</summary>
    public string Name { get; init; }

    public List<TimeActMergeSourceFile> Files { get; } = new();

    public List<TimeActMergeProblem> Problems { get; } = new();

    /// <summary>Why the source couldn't be used at all, or null</summary>
    public TimeActMergeProblem Error { get; init; }

    public bool IsValid => Error == null;

    /// <summary>The file system the source reads from (a project's folder, a mod folder, a loose file's folder)</summary>
    public VirtualFileSystem FileSystem { get; init; }

    public bool OwnsFileSystem { get; init; }

    public void Dispose()
    {
        if (OwnsFileSystem)
            FileSystem?.Dispose();
    }

    public static TimeActMergeSource Failed(TimeActMergeSourceType type, string name, string code, string detail = null) =>
        new() { Type = type, Name = name, Error = new TimeActMergeProblem(code, name, detail) };

    /// <summary>The anibnds under /chr (not a variant's split anibnd, which holds clips only), as a source has them</summary>
    public static List<string> FindCharacterAnibnds(VirtualFileSystem fs)
    {
        try
        {
            return fs.GetFileNamesMatchingRecursive("/chr", @"(?i)\.anibnd(\.dcx)?$")
                .Select(p => "/" + p.Replace('\\', '/').TrimStart('/'))
                .Where(p => !System.IO.Path.GetFileName(p).Contains('_'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception)
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// A mod folder's root: the folder chosen, its parent or its grandparent, the first holding one of this project's
    /// source paths; else the first holding any character anibnd. Null if none does.
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
            if (expected.Any(p => File.Exists(System.IO.Path.Combine(candidate, p))))
                return candidate;
        }

        foreach (var candidate in candidates)
        {
            var chr = System.IO.Path.Combine(candidate, "chr");

            if (Directory.Exists(chr) && Directory.EnumerateFiles(chr, "*.anibnd*", SearchOption.AllDirectories).Any())
                return candidate;
        }

        return null;
    }
}

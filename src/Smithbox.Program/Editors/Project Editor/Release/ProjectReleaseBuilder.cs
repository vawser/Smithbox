using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StudioCore.Application;

/// <summary>
/// Why a file or folder of the project folder is left out of a release.
/// </summary>
public enum ReleaseExclusionReason
{
    None,
    /// <summary>A file or folder whose name starts with '.': Smithbox's own data (.smithbox, .backup), .git, .vs</summary>
    HiddenData,
    /// <summary>.bak, .prev, .temp and the like</summary>
    Backup,
    /// <summary>A folder unpacked by WitchyBND or Yabber</summary>
    UnpackedFolder,
    /// <summary>A text form of a game file written by an unpacker or script tool (a .fmg.xml, an .emevd.dcx.js)</summary>
    SerializedCopy,
    /// <summary>An archive, such as an earlier release</summary>
    Archive,
    /// <summary>A file the system or another tool writes: Thumbs.db, desktop.ini, .log</summary>
    SystemFile,
    /// <summary>The project file of a legacy Smithbox project</summary>
    ProjectFile,
    /// <summary>Matched one of the user's exclusion patterns</summary>
    UserPattern
}

/// <summary>
/// One file or folder found in the project folder. A left-out folder is one entry for all it holds.
/// </summary>
public class ReleaseEntry
{
    /// <summary>Relative to the project folder, with '/' separators</summary>
    public string RelativePath;
    public string FullPath;
    public bool IsFolder;
    public int FileCount = 1;
    public long Size;
    public ReleaseExclusionReason Reason;

    /// <summary>For <see cref="ReleaseExclusionReason.UserPattern"/>: the pattern it matched</summary>
    public string Pattern = "";

    public bool Included => Reason is ReleaseExclusionReason.None;
}

/// <summary>
/// What a scan of the project folder found: the files a release holds and the ones it leaves out.
/// </summary>
public class ReleaseScanResult
{
    public string Root = "";
    public List<ReleaseEntry> Included = new();
    public List<ReleaseEntry> Excluded = new();

    public long IncludedSize => Included.Sum(e => e.Size);
    public int ExcludedFileCount => Excluded.Sum(e => e.FileCount);
    public long ExcludedSize => Excluded.Sum(e => e.Size);
}

/// <summary>
/// The release settings of a project, kept with it in &lt;project&gt;/.smithbox/Project/Release.json.
/// </summary>
public class ProjectReleaseSettings
{
    /// <summary>The folder the ZIP is written to. Empty: the folder that holds the project folder.</summary>
    public string OutputFolder { get; set; } = "";

    /// <summary>The ZIP's file name, without .zip. Empty: the project's name.</summary>
    public string ArchiveName { get; set; } = "";

    /// <summary>A folder in the ZIP that holds the files (such as "mod"). Empty: the files are at the ZIP's root.</summary>
    public string RootFolder { get; set; } = "";

    /// <summary>Patterns of the files and folders to leave out, besides the built-in rules</summary>
    public List<string> ExcludePatterns { get; set; } = new();
}

/// <summary>
/// Scans a project folder for the files the game reads, and writes them to a release ZIP. Leaves out what only
/// the modder's tools use: Smithbox's .smithbox and .backup folders, backups (.bak, .prev), folders unpacked by
/// WitchyBND or Yabber, the text copies those tools write beside a file, and earlier archives.
/// </summary>
public static class ProjectReleaseBuilder
{
    private static readonly HashSet<string> BackupExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        // .partial: a release ZIP being written
        ".bak", ".prev", ".temp", ".tmp", ".old", ".backup", ".partial"
    };

    private static readonly HashSet<string> ArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".7z", ".rar"
    };

    private static readonly HashSet<string> SystemFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Thumbs.db", "desktop.ini"
    };

    private static readonly HashSet<string> SystemFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".log"
    };

    // The extensions of the text forms tools write beside a game file: WitchyBND's .xml / .json, DarkScript's .js,
    // ESDLang's .py, a param .csv. A file with one of these is a copy when the file it is named after is beside it.
    private static readonly HashSet<string> SerializedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".xml", ".json", ".js", ".py", ".csv", ".yaml", ".yml"
    };

    // Formats WitchyBND serializes to <name>.<format>.xml (or .json): a copy even without the original beside it, as
    // the game never reads them in that form
    private static readonly HashSet<string> SerializedFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        "fmg", "param", "gparam", "matbin", "mtd", "fxr", "luagnl", "luainfo", "mqb", "msb", "tae", "dbsub",
        "paramdef", "btab", "btl", "anc", "buddy"
    };

    // WitchyBND and Yabber write one of these into each folder they unpack
    private static readonly Regex UnpackManifest = new(@"^_(witchy|yabber)-.+\.xml$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    #region Settings

    public static string SettingsPath(ProjectEntry project)
        => Path.Join(project.Descriptor.ProjectPath, ".smithbox", "Project", "Release.json");

    public static ProjectReleaseSettings LoadSettings(ProjectEntry project)
    {
        var path = SettingsPath(project);

        try
        {
            if (File.Exists(path))
                return JsonSerializer.Deserialize<ProjectReleaseSettings>(File.ReadAllText(path)) ?? new();
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(ProjectReleaseBuilder), LOC.Get("PROJECT_Release_Settings_Load_Failed", path), e);
        }

        return new();
    }

    public static void SaveSettings(ProjectEntry project, ProjectReleaseSettings settings)
    {
        var path = SettingsPath(project);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(ProjectReleaseBuilder), LOC.Get("PROJECT_Release_Settings_Save_Failed", path), e);
        }
    }

    /// <summary>The folder the ZIP goes to: the setting, or else the folder that holds the project folder</summary>
    public static string GetOutputFolder(ProjectEntry project, ProjectReleaseSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.OutputFolder))
            return settings.OutputFolder.Trim();

        var projectPath = Path.TrimEndingDirectorySeparator(project.Descriptor.ProjectPath ?? "");

        return Path.GetDirectoryName(projectPath) ?? projectPath;
    }

    /// <summary>The ZIP's file name, with .zip, made safe for a file name</summary>
    public static string GetArchiveFileName(ProjectEntry project, ProjectReleaseSettings settings)
    {
        var name = string.IsNullOrWhiteSpace(settings.ArchiveName) ? project.Descriptor.ProjectName : settings.ArchiveName.Trim();

        if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];

        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');

        if (string.IsNullOrWhiteSpace(name))
            name = "Release";

        return $"{name}.zip";
    }

    /// <summary>The folder inside the ZIP, cleaned to a relative path with '/' and no leading or trailing '/'</summary>
    public static string GetRootFolder(ProjectReleaseSettings settings)
    {
        var root = (settings.RootFolder ?? "").Replace('\\', '/').Trim().Trim('/');

        // No climbing out of the archive
        var parts = root.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p != "." && p != "..");

        return string.Join('/', parts);
    }

    #endregion

    #region Scan

    /// <summary>
    /// Walks the project folder, sorting every file into included or excluded. A left-out folder isn't walked into;
    /// it is one excluded entry.
    /// </summary>
    public static ReleaseScanResult Scan(string root, IReadOnlyList<string> userPatterns, CancellationToken token)
    {
        var result = new ReleaseScanResult { Root = root };

        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            return result;

        var patterns = userPatterns
            .Select(p => p?.Trim())
            .Where(p => !string.IsNullOrEmpty(p) && !p.StartsWith('#'))
            .Select(p => (Text: p, Regex: PatternToRegex(p)))
            .ToList();

        ScanFolder(root, "", patterns, result, token);

        result.Included.Sort((a, b) => string.Compare(a.RelativePath, b.RelativePath, StringComparison.OrdinalIgnoreCase));
        result.Excluded.Sort((a, b) => string.Compare(a.RelativePath, b.RelativePath, StringComparison.OrdinalIgnoreCase));

        return result;
    }

    private static void ScanFolder(string folder, string relativeFolder, List<(string Text, Regex Regex)> patterns,
        ReleaseScanResult result, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        string[] files;
        string[] folders;

        try
        {
            files = Directory.GetFiles(folder);
            folders = Directory.GetDirectories(folder);
        }
        catch (Exception e) when (e is UnauthorizedAccessException or IOException)
        {
            Smithbox.LogWarning(typeof(ProjectReleaseBuilder), LOC.Get("PROJECT_Release_Scan_Folder_Skipped", folder, e.Message));
            return;
        }

        var siblingNames = new HashSet<string>(files.Select(Path.GetFileName), StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            var relative = relativeFolder.Length == 0 ? name : $"{relativeFolder}/{name}";

            var entry = new ReleaseEntry
            {
                RelativePath = relative,
                FullPath = file,
                Size = GetFileSize(file)
            };

            entry.Reason = GetFileReason(name, relative, relativeFolder.Length == 0, siblingNames, patterns, out entry.Pattern);

            (entry.Included ? result.Included : result.Excluded).Add(entry);
        }

        foreach (var sub in folders)
        {
            var name = Path.GetFileName(sub);
            var relative = relativeFolder.Length == 0 ? name : $"{relativeFolder}/{name}";

            var reason = GetFolderReason(sub, name, relative, siblingNames, patterns, out var pattern);

            if (reason is ReleaseExclusionReason.None)
            {
                ScanFolder(sub, relative, patterns, result, token);
                continue;
            }

            var (count, size) = CountFolder(sub, token);

            result.Excluded.Add(new ReleaseEntry
            {
                RelativePath = $"{relative}/",
                FullPath = sub,
                IsFolder = true,
                FileCount = count,
                Size = size,
                Reason = reason,
                Pattern = pattern
            });
        }
    }

    private static ReleaseExclusionReason GetFileReason(string name, string relative, bool atRoot,
        HashSet<string> siblingNames, List<(string Text, Regex Regex)> patterns, out string pattern)
    {
        pattern = "";

        if (name.StartsWith('.'))
            return ReleaseExclusionReason.HiddenData;

        var ext = Path.GetExtension(name);

        if (BackupExtensions.Contains(ext))
            return ReleaseExclusionReason.Backup;

        if (ArchiveExtensions.Contains(ext))
            return ReleaseExclusionReason.Archive;

        if (SystemFileNames.Contains(name) || SystemFileExtensions.Contains(ext))
            return ReleaseExclusionReason.SystemFile;

        if (UnpackManifest.IsMatch(name))
            return ReleaseExclusionReason.UnpackedFolder;

        if (IsSerializedCopy(name, ext, siblingNames))
            return ReleaseExclusionReason.SerializedCopy;

        // A legacy project kept its project.json in the project folder
        if (atRoot && name.Equals("project.json", StringComparison.OrdinalIgnoreCase))
            return ReleaseExclusionReason.ProjectFile;

        if (MatchesUserPattern(relative, name, false, patterns, out pattern))
            return ReleaseExclusionReason.UserPattern;

        return ReleaseExclusionReason.None;
    }

    private static ReleaseExclusionReason GetFolderReason(string fullPath, string name, string relative,
        HashSet<string> siblingNames, List<(string Text, Regex Regex)> patterns, out string pattern)
    {
        pattern = "";

        if (name.StartsWith('.'))
            return ReleaseExclusionReason.HiddenData;

        if (IsUnpackedFolder(fullPath, name, siblingNames))
            return ReleaseExclusionReason.UnpackedFolder;

        if (MatchesUserPattern(relative, name, true, patterns, out pattern))
            return ReleaseExclusionReason.UserPattern;

        return ReleaseExclusionReason.None;
    }

    /// <summary>
    /// A folder WitchyBND or Yabber unpacked: it holds their manifest, or is named after a file beside it with the
    /// dots turned to dashes (c0000.anibnd.dcx unpacks to c0000-anibnd-dcx).
    /// </summary>
    private static bool IsUnpackedFolder(string fullPath, string name, HashSet<string> siblingNames)
    {
        try
        {
            foreach (var file in Directory.EnumerateFiles(fullPath, "_*.xml"))
            {
                if (UnpackManifest.IsMatch(Path.GetFileName(file)))
                    return true;
            }
        }
        catch (Exception e) when (e is UnauthorizedAccessException or IOException)
        {
            return false;
        }

        // Only the last dashes are dots, as a name can hold dashes of its own
        var candidate = name;

        for (var i = 0; i < 3; i++)
        {
            var dash = candidate.LastIndexOf('-');
            if (dash <= 0)
                break;

            candidate = $"{candidate[..dash]}.{candidate[(dash + 1)..]}";

            if (siblingNames.Contains(candidate))
                return true;
        }

        return false;
    }

    /// <summary>
    /// A text copy of a game file: named after a file beside it (common.emevd.dcx.js beside common.emevd.dcx), or a
    /// WitchyBND serialization (item.fmg.xml).
    /// </summary>
    private static bool IsSerializedCopy(string name, string ext, HashSet<string> siblingNames)
    {
        if (!SerializedExtensions.Contains(ext))
            return false;

        var stem = name[..^ext.Length];

        if (siblingNames.Contains(stem))
            return true;

        // The format is the last extension of the stem, past a .dcx (item.fmg.dcx.xml)
        var inner = Path.GetExtension(stem);

        if (inner.Equals(".dcx", StringComparison.OrdinalIgnoreCase))
            inner = Path.GetExtension(stem[..^inner.Length]);

        return inner.Length > 1 && SerializedFormats.Contains(inner[1..]);
    }

    private static bool MatchesUserPattern(string relative, string name, bool isFolder,
        List<(string Text, Regex Regex)> patterns, out string matched)
    {
        matched = "";

        foreach (var (text, regex) in patterns)
        {
            var folderOnly = text.EndsWith('/');

            if (folderOnly && !isFolder)
                continue;

            // A pattern without a '/' (other than a trailing one) matches a name anywhere; one with a '/' matches the
            // path from the project folder
            var hasSlash = text.TrimEnd('/').Contains('/');

            if (regex.IsMatch(hasSlash ? relative : name))
            {
                matched = text;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// A wildcard pattern as a regex: * is any text within a name, ** is any text across folders, ? one character.
    /// </summary>
    public static Regex PatternToRegex(string pattern)
    {
        var text = pattern.Replace('\\', '/').Trim().TrimEnd('/').TrimStart('/');

        var regex = new System.Text.StringBuilder("^");

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (c == '*')
            {
                if (i + 1 < text.Length && text[i + 1] == '*')
                {
                    regex.Append(".*");
                    i++;

                    // **/ also matches no folder at all
                    if (i + 1 < text.Length && text[i + 1] == '/')
                    {
                        regex.Append("/?");
                        i++;
                    }
                }
                else
                {
                    regex.Append("[^/]*");
                }
            }
            else if (c == '?')
            {
                regex.Append("[^/]");
            }
            else
            {
                regex.Append(Regex.Escape(c.ToString()));
            }
        }

        regex.Append('$');

        return new Regex(regex.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static (int Count, long Size) CountFolder(string folder, CancellationToken token)
    {
        var count = 0;
        long size = 0;

        try
        {
            foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                token.ThrowIfCancellationRequested();

                count++;
                size += GetFileSize(file);
            }
        }
        catch (Exception e) when (e is UnauthorizedAccessException or IOException)
        {
            // Counts what could be read
        }

        return (count, size);
    }

    private static long GetFileSize(string file)
    {
        try
        {
            return new FileInfo(file).Length;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    #endregion

    #region Write

    /// <summary>
    /// Writes the included files to a ZIP. It is written beside the target under a temporary name and moved over the
    /// target once complete, so a failed or cancelled build never leaves a broken ZIP behind.
    /// </summary>
    /// <param name="progress">Called with the number of files written so far</param>
    public static void WriteZip(ReleaseScanResult scan, string zipPath, string rootFolder, Action<int> progress,
        CancellationToken token)
    {
        var tempPath = $"{zipPath}.partial";

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(zipPath));

            using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                var written = 0;

                foreach (var entry in scan.Included)
                {
                    token.ThrowIfCancellationRequested();

                    var name = rootFolder.Length == 0 ? entry.RelativePath : $"{rootFolder}/{entry.RelativePath}";

                    zip.CreateEntryFromFile(entry.FullPath, name, CompressionLevel.Optimal);

                    written++;
                    progress?.Invoke(written);
                }
            }

            File.Move(tempPath, zipPath, true);
        }
        catch
        {
            try
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
            catch (Exception)
            {
                // The partial file is left; the next build overwrites it
            }

            throw;
        }
    }

    #endregion
}

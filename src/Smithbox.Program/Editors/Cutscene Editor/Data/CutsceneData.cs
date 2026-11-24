#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// Holds the data for the Cutscene Editor: the project's cutscene binders, each read on first use.
/// </summary>
public class CutsceneData : IDisposable
{
    public ProjectEntry Project;

    /// <summary>Every cutscene binder of the project, by name</summary>
    public List<CutsceneFile> Files { get; private set; } = new();

    /// <summary>
    /// The project files of deleted (or renamed) cutscenes the game doesn't have, moved into the project's backup
    /// folder on the next save, with their camera keys
    /// </summary>
    public List<(string Path, string Name)> PendingDeletes { get; } = new();

    private static readonly Regex NamePattern = new(@"^s\d{2}_\d{2}_\d{4}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public CutsceneData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        try
        {
            Files = Project.Locator.FileDictionary.Entries
                .Where(e => e.Extension is "cutscenebnd" or "remobnd")
                .GroupBy(e => e.Filename, StringComparer.OrdinalIgnoreCase)
                .Select(g => new CutsceneFile(g.First()))
                .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("CUTSCENE_Data_Setup_FAIL"), e);
            return false;
        }

        Smithbox.Log(this, LOC.Get("CUTSCENE_Data_Setup_PASS", Files.Count));
        return true;
    }

    // ---- the list

    /// <summary>Adds a file at its sorted place, or at an index (an undone delete); returns the index</summary>
    public int Insert(CutsceneFile file, int index = -1)
    {
        if (index < 0)
        {
            index = Files.FindIndex(f => string.Compare(f.Name, file.Name, StringComparison.OrdinalIgnoreCase) > 0);
            if (index < 0)
                index = Files.Count;
        }

        index = Math.Clamp(index, 0, Files.Count);
        Files.Insert(index, file);
        return index;
    }

    /// <summary>Takes a file out of the list; returns where it was</summary>
    public int Remove(CutsceneFile file)
    {
        var index = Files.IndexOf(file);
        if (index >= 0)
            Files.RemoveAt(index);
        return index;
    }

    /// <summary>The VFS path a cutscene of another name would have, beside this one: /cutscene/s10_00_0020.cutscenebnd.dcx</summary>
    public static string PathFor(CutsceneFile beside, string name)
    {
        var path = beside.Entry.Path;
        var slash = path.LastIndexOf('/');
        var fileName = path[(slash + 1)..];
        var oldStem = fileName.Split('.')[0];
        return path[..(slash + 1)] + name + fileName[oldStem.Length..];
    }

    /// <summary>A file dictionary entry for a cutscene of another name, beside this one</summary>
    public static FileDictionaryEntry EntryFor(CutsceneFile beside, string name)
    {
        var entry = beside.Entry.Clone();
        entry.Path = PathFor(beside, name);
        entry.Filename = name;
        return entry;
    }

    /// <summary>
    /// Why a cutscene can't be given a name, or null (LOC key). Cutscenes are played by ID, so the name must be one
    /// (sAA_BB_CCCC), and not one the list, the project, or the game already has.
    /// </summary>
    public string? NameRefusal(CutsceneFile beside, string name, CutsceneFile? renaming = null)
    {
        if (renaming != null && string.Equals(renaming.Name, name, StringComparison.Ordinal))
            return "CUTSCENE_File_No_Change";
        if (!NamePattern.IsMatch(name))
            return "CUTSCENE_File_Name_Shape";

        var path = PathFor(beside, name);
        if (Files.Any(f => f != renaming && string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase))
            || PendingDeletes.Any(d => string.Equals(d.Path, path, StringComparison.OrdinalIgnoreCase))
            || (Project.VFS.FS?.FileExists(path) ?? false))
            return "CUTSCENE_File_Name_Taken";

        return null;
    }

    /// <summary>A free name for a new cutscene like this one: the next free number in its map (s10_00_0020 → s10_00_0021)</summary>
    public string SuggestName(CutsceneFile beside)
    {
        var m = Regex.Match(beside.Name, @"^(s\d{2}_\d{2}_)(\d{4})$", RegexOptions.IgnoreCase);
        var prefix = m.Success ? m.Groups[1].Value : "s00_00_";
        var start = m.Success ? int.Parse(m.Groups[2].Value) + 1 : 0;
        for (var i = 0; i < 10000; i++)
        {
            var candidate = $"{prefix}{(start + i) % 10000:D4}";
            if (NameRefusal(beside, candidate) == null)
                return candidate;
        }

        return beside.Name;
    }

    // ---- saving

    /// <summary>Whether the next save moves project files away (deleted, renamed or reverted cutscenes)</summary>
    public bool HasFileRemovals => PendingDeletes.Count > 0 || Files.Any(f => f.PendingRevert);

    /// <summary>
    /// Moves the project files of deleted cutscenes, and of reverted ones, into the project's backup folder (a
    /// reverted one then reads the game's file again). Returns false if any failed.
    /// </summary>
    public bool ApplyRemovals()
    {
        var ok = true;

        foreach (var pending in PendingDeletes.ToList())
        {
            if (MoveToBackup(pending.Path, pending.Name))
                PendingDeletes.Remove(pending);
            else
                ok = false;
        }

        foreach (var file in Files.Where(f => f.PendingRevert).ToList())
        {
            if (MoveToBackup(file.Entry.Path, file.Name))
                file.ResetToVanilla();
            else
                ok = false;
        }

        return ok;
    }

    /// <summary>
    /// Moves a project file into &lt;project&gt;/.backup/&lt;time&gt;_deleted/&lt;its path&gt; (the project scan skips dot
    /// folders), and the cutscene's camera keys beside it. Nothing to do when the project has neither.
    /// </summary>
    private bool MoveToBackup(string path, string name)
    {
        var projectFs = Project.VFS.ProjectFS;
        var stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_deleted";
        try
        {
            if (projectFs.FileExists(path))
            {
                var relative = $".backup/{stamp}{path}";
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Join(Project.Descriptor.ProjectPath, relative))!);
                projectFs.Move(path, relative);
                Smithbox.Log(this, LOC.Get("CUTSCENE_File_Moved_To_Backup", path, relative));
            }

            var keys = CutsceneCameraKeyStore.PathFor(Project, name);
            if (File.Exists(keys))
            {
                var target = Path.Join(Project.Descriptor.ProjectPath, ".backup", stamp, ".smithbox", "Cutscenes", Path.GetFileName(keys));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Move(keys, target, true);
            }

            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("CUTSCENE_File_Move_To_Backup_Failed", path), e);
            return false;
        }
    }

    public void Dispose()
    {
        Files = new();
        GC.SuppressFinalize(this);
    }
}

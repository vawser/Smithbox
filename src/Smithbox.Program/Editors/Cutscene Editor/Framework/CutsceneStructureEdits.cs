#nullable enable
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// An undoable change to the cutscene list (a cutscene added, pasted, renamed or deleted), as a pair of closures. What
/// it means for the project's files happens on save: <see cref="CutsceneData.ApplyRemovals" /> and
/// <see cref="CutsceneFile.Save" />.
/// </summary>
public class CutsceneFileAction : EditorAction
{
    private readonly Action Do;
    private readonly Action UndoChange;

    public CutsceneFileAction(Action execute, Action undo)
    {
        Do = execute;
        UndoChange = undo;
    }

    public override ActionEvent Execute()
    {
        Do();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        UndoChange();
        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Adding, pasting, renaming and deleting whole cutscenes. Each returns its action, or null with the reason logged.
/// Every one is undoable, and written to the project on save:
/// <list type="bullet">
/// <item>Added and pasted cutscenes are new files, built from the binder of the one they came from (or were made
/// beside), so they keep its header.</item>
/// <item>A renamed one becomes a new file too. If the game has the old name, it shows the game's cutscene again; if
/// only the project had it, the project's file goes into the backup folder.</item>
/// <item>Deleting a cutscene the game has reverts it: the project's copy goes into the backup folder. One only the
/// project has is moved there; a new one just goes. One only the game has can't be deleted.</item>
/// </list>
/// </summary>
public static class CutsceneFileOps
{
    /// <summary>A new cutscene with one empty cut, beside another (its folder, binder header, MQB version and framerate)</summary>
    public static CutsceneFileAction? AddBlank(CutsceneData data, CutsceneFile beside, string name)
    {
        if (Refused(data.NameRefusal(beside, name), name))
            return null;

        var project = data.Project;
        var raw = beside.ReadRaw(project);
        var template = beside.ReadMqbOnly(project);
        if (raw == null || template == null)
        {
            Smithbox.LogError(typeof(CutsceneFileOps), LOC.Get("CUTSCENE_File_Template_Failed", beside.Entry.Path));
            return null;
        }

        var file = CutsceneFile.CreateBlank(CutsceneData.EntryFor(beside, name), raw, template);
        return new CutsceneFileAction(() => data.Insert(file), () => data.Remove(file));
    }

    /// <summary>A copy of a cutscene under a new name, its unsaved edits included</summary>
    public static CutsceneFileAction? Paste(CutsceneData data, CutsceneFile source, string name)
    {
        if (Refused(data.NameRefusal(source, name), name))
            return null;

        var project = data.Project;
        if (!source.Load(project))
        {
            Smithbox.LogError(typeof(CutsceneFileOps), LOC.Get("CUTSCENE_File_Not_Loaded", source.Name));
            return null;
        }

        CutsceneFile copy;
        try
        {
            copy = source.CopyAs(project, CutsceneData.EntryFor(source, name));
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(CutsceneFileOps), LOC.Get("CUTSCENE_File_Copy_Failed", source.Name), e);
            return null;
        }

        return new CutsceneFileAction(() => data.Insert(copy), () => data.Remove(copy));
    }

    /// <summary>Why a cutscene can't be renamed, or null (LOC key)</summary>
    public static string? RenameRefusal(CutsceneFile file) => file.PendingRevert ? "CUTSCENE_File_Pending_Revert" : null;

    public static CutsceneFileAction? Rename(CutsceneData data, CutsceneFile file, string name)
    {
        var refusal = RenameRefusal(file) ?? data.NameRefusal(file, name, file);
        if (refusal == "CUTSCENE_File_No_Change" || Refused(refusal, name))
            return null;

        var project = data.Project;
        if (!file.Load(project))
        {
            Smithbox.LogError(typeof(CutsceneFileOps), LOC.Get("CUTSCENE_File_Not_Loaded", file.Name));
            return null;
        }

        var before = file.CaptureIdentity();
        var wasDirty = file.IsDirty;
        var newEntry = CutsceneData.EntryFor(file, name);

        // What becomes of the old name: the game's cutscene shows again (the project's copy going on save), or the
        // project's file goes on save. A new file leaves nothing behind.
        var oldPath = file.Entry.Path;
        var inProject = !file.IsNew && project.VFS.ProjectFS.FileExists(oldPath);
        var inGame = !file.IsNew && (project.VFS.VanillaFS?.FileExists(oldPath) ?? false);
        var placeholder = inGame ? new CutsceneFile(file.Entry.Clone()) { PendingRevert = inProject } : null;
        (string, string)? pendingDelete = !inGame && inProject ? (oldPath, file.Name) : null;

        var index = data.Files.IndexOf(file);

        return new CutsceneFileAction(
            () =>
            {
                file.Rename(project, newEntry);
                file.IsDirty = true;
                data.Remove(file);
                data.Insert(file);
                if (placeholder != null)
                    data.Insert(placeholder);
                if (pendingDelete is { } d)
                    data.PendingDeletes.Add(d);
            },
            () =>
            {
                if (pendingDelete is { } d)
                    data.PendingDeletes.Remove(d);
                if (placeholder != null)
                    data.Remove(placeholder);
                data.Remove(file);
                file.RestoreIdentity(before);
                file.IsDirty = wasDirty;
                data.Insert(file, index);
            });
    }

    /// <summary>Why a cutscene can't be deleted, or null (LOC key): one only the game has would still be loaded</summary>
    public static string? DeleteRefusal(CutsceneData data, CutsceneFile file)
    {
        if (file.PendingRevert)
            return "CUTSCENE_File_Pending_Revert";
        if (!file.IsNew && !data.Project.VFS.ProjectFS.FileExists(file.Entry.Path))
            return "CUTSCENE_File_Delete_Vanilla_Refused";
        return null;
    }

    /// <summary>The tooltip of Delete: what it would do to this file</summary>
    public static string DeleteTooltip(CutsceneData data, CutsceneFile file) =>
        DeleteRefusal(data, file) ?? (file.IsNew ? "CUTSCENE_File_Delete_New_TT"
            : data.Project.VFS.VanillaFS?.FileExists(file.Entry.Path) == true ? "CUTSCENE_File_Delete_Revert_TT"
            : "CUTSCENE_File_Delete_Project_TT");

    public static CutsceneFileAction? Delete(CutsceneData data, CutsceneFile file)
    {
        if (Refused(DeleteRefusal(data, file), file.Name))
            return null;

        if (!file.IsNew && (data.Project.VFS.VanillaFS?.FileExists(file.Entry.Path) ?? false))
            return new CutsceneFileAction(() => file.PendingRevert = true, () => file.PendingRevert = false);

        var index = data.Files.IndexOf(file);
        (string, string)? pendingDelete = file.IsNew ? null : (file.Entry.Path, file.Name);

        return new CutsceneFileAction(
            () =>
            {
                data.Remove(file);
                if (pendingDelete is { } d)
                    data.PendingDeletes.Add(d);
            },
            () =>
            {
                if (pendingDelete is { } d)
                    data.PendingDeletes.Remove(d);
                data.Insert(file, index);
            });
    }

    private static bool Refused(string? refusal, string name)
    {
        if (refusal == null)
            return false;

        Smithbox.Log(typeof(CutsceneFileOps), LOC.Get(refusal, name), Microsoft.Extensions.Logging.LogLevel.Warning);
        return true;
    }
}

/// <summary>A cut copied with everything it needs to be pasted into any cutscene: what <see cref="CutsceneCutOps.Copy" /> takes</summary>
public sealed class CutsceneCutClipboard
{
    public required CutsceneFile SourceFile;
    public required string SourceName;
    public required float Framerate;

    /// <summary>A copy of the cut; its events' ResourceIndex are the source file's</summary>
    public required MQB.Cut Cut;

    /// <summary>The resources the cut's events use, and their parents: the source's object, and a copy of it, by source index</summary>
    public required SortedDictionary<int, (MQB.Resource Original, MQB.Resource Copy)> Resources;

    /// <summary>The Havok files and camera keys of the cut's HKT folders, by folder</summary>
    public required Dictionary<string, CutsceneFile.CutHavok> Havok;
}

/// <summary>
/// Adding, renaming, deleting, copying and pasting the cuts of a cutscene, as undoable edits written on save. What
/// the vanilla files keep (Claude/Tools/CutsceneNamingSurvey, every game with MQBs):
/// <list type="bullet">
/// <item>A cut is named cutNNNN, unique in its file. Its Havok files are in a binder folder of its name
/// (...\s10_00_0020\cut0010\a0000.hkx), which its HKT resources name too (...\scn\s10_00_0020\cut0010\hkx\a0000.HKT).</item>
/// <item>Actors, cameras, offsets and HKTs (types 1, 3, 265, 266), and whatever is parented to them, belong to one cut.
/// Other resources (effects, sounds, lights...) may be shared by several.</item>
/// <item>Event IDs are unique in the file.</item>
/// </list>
/// </summary>
public static class CutsceneCutOps
{
    private static readonly Regex CutName = new(@"^cut(\d{4})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static int? CutNumber(string name) => CutName.Match(name) is { Success: true } m ? int.Parse(m.Groups[1].Value) : null;

    /// <summary>Why a cut can't be given a name, or null (LOC key)</summary>
    public static string? NameRefusal(CutsceneFile file, string name, MQB.Cut? renaming = null)
    {
        if (renaming != null && string.Equals(renaming.Name, name, StringComparison.Ordinal))
            return "CUTSCENE_File_No_Change";
        if (!CutName.IsMatch(name))
            return "CUTSCENE_Cut_Name_Shape";

        var mqb = file.Mqb!;
        if (mqb.Cuts.Any(c => c != renaming && string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            return "CUTSCENE_Cut_Name_Taken";

        // A folder of that name still holding Havok files, of a cut that is deleted or names it otherwise
        if (file.HasCutFolder(name) && !(renaming != null && string.Equals(renaming.Name, name, StringComparison.OrdinalIgnoreCase)))
            return "CUTSCENE_Cut_Name_Taken";

        return null;
    }

    /// <summary>A free name for a cut after the one at <paramref name="after" /> (-1: at the start), in between it and the next if it can be</summary>
    public static string SuggestName(CutsceneFile file, int after)
    {
        var cuts = file.Mqb!.Cuts;
        var prev = after >= 0 && after < cuts.Count ? CutNumber(cuts[after].Name) ?? 0 : 0;
        var next = after + 1 < cuts.Count ? CutNumber(cuts[after + 1].Name) : null;

        foreach (var step in new[] { 10, 5, 1 })
        {
            var n = (prev / step + 1) * step;
            if (n < 10000 && (next == null || n < next) && NameRefusal(file, $"cut{n:D4}") == null)
                return $"cut{n:D4}";
        }

        for (var n = 10; n < 10000; n += 10)
            if (NameRefusal(file, $"cut{n:D4}") == null)
                return $"cut{n:D4}";

        return "cut0000";
    }

    // ---- which resources a cut uses

    /// <summary>The resources a cut's events use, with their parents, by index</summary>
    public static SortedSet<int> UsedResources(MQB mqb, MQB.Cut cut)
    {
        var used = new SortedSet<int>();
        foreach (var timeline in cut.Timelines)
            foreach (var e in timeline.Events)
            {
                var ri = e.ResourceIndex;
                while (ri >= 0 && ri < mqb.Resources.Count && used.Add(ri))
                    ri = mqb.Resources[ri].ParentIndex;
            }

        return used;
    }

    /// <summary>The HKT folders (cutNNNN) of a cut: those of the HKTs it uses</summary>
    private static HashSet<string> HavokFolders(MQB mqb, IEnumerable<int> resources) =>
        resources.Where(i => mqb.Resources[i].Type == CutsceneResourceTypes.Animation)
            .Select(i => CutsceneFile.CutFolderOf(mqb.Resources[i].Path))
            .OfType<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Whether a resource belongs to one cut: an actor, camera, offset or HKT, or anything under one</summary>
    private static bool IsPerCut(MQB mqb, int index)
    {
        // Bounded, in case a file's parents loop
        for (int i = index, steps = 0; i >= 0 && i < mqb.Resources.Count && steps < mqb.Resources.Count; i = mqb.Resources[i].ParentIndex, steps++)
        {
            if (mqb.Resources[i].Type is CutsceneResourceTypes.Model or CutsceneResourceTypes.Camera
                or CutsceneResourceTypes.Offset or CutsceneResourceTypes.Animation)
                return true;
        }

        return false;
    }

    // ---- deep copies

    private static MQB.Cut Clone(MQB.Cut cut) => new()
    {
        Name = cut.Name,
        Unk44 = cut.Unk44,
        Duration = cut.Duration,
        Timelines = cut.Timelines.Select(t => new MQB.Timeline
        {
            ParentIndex = t.ParentIndex,
            Parameters = t.Parameters.Select(CutsceneMqbOps.Clone).ToList(),
            Events = t.Events.Select(CutsceneMqbOps.Clone).ToList()
        }).ToList()
    };

    // ---- the edits

    /// <summary>A new empty cut (five seconds) at an index</summary>
    public static CutsceneEditAction? Add(CutsceneFile file, int index, string name)
    {
        if (Refused(NameRefusal(file, name), name))
            return null;

        var mqb = file.Mqb!;
        var cut = new MQB.Cut { Name = name, Duration = (int)((mqb.Framerate > 0 ? mqb.Framerate : 30) * 5) };
        index = Math.Clamp(index, 0, mqb.Cuts.Count);

        return new CutsceneEditAction(file, () => mqb.Cuts.Insert(index, cut), () => mqb.Cuts.Remove(cut));
    }

    public static CutsceneEditAction? Rename(ProjectEntry project, CutsceneFile file, MQB.Cut cut, string name)
    {
        var refusal = NameRefusal(file, name, cut);
        if (refusal == "CUTSCENE_File_No_Change" || Refused(refusal, name))
            return null;

        var mqb = file.Mqb!;
        var old = cut.Name;

        // The cut's own folder moves with it: the HKTs naming it, and its Havok files and camera keys. Not one another
        // cut's HKTs name too, or one the cut doesn't use (some AC6 cuts play the HKT of a folder named otherwise).
        var mine = UsedResources(mqb, cut);
        var others = mqb.Cuts.Where(c => c != cut).SelectMany(c => UsedResources(mqb, c)).ToHashSet();
        var hkts = mine.Where(i => mqb.Resources[i].Type == CutsceneResourceTypes.Animation
                                   && string.Equals(CutsceneFile.CutFolderOf(mqb.Resources[i].Path), old, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var moveFolder = hkts.Count > 0 && !hkts.Any(others.Contains) || hkts.Count == 0 && file.HasCutFolder(old) && !HavokFolders(mqb, others).Contains(old);
        if (!moveFolder)
            hkts.Clear();

        var paths = hkts.ToDictionary(i => mqb.Resources[i], i => mqb.Resources[i].Path);
        CutsceneFile.CutHavok? havok = null;

        return new CutsceneEditAction(file,
            () =>
            {
                cut.Name = name;
                foreach (var (r, path) in paths)
                    r.Path = CutsceneFile.ReplaceFolder(path, old, name);

                if (moveFolder)
                {
                    havok ??= file.TakeCut(project, old);
                    file.RemoveCut(old);
                    file.PutCut(project, name, havok);
                }
            },
            () =>
            {
                cut.Name = old;
                foreach (var (r, path) in paths)
                    r.Path = path;

                if (moveFolder && havok != null)
                {
                    file.RemoveCut(name);
                    file.PutCut(project, old, havok);
                }
            });
    }

    /// <summary>Why a cut can't be deleted, or null (LOC key): a cutscene keeps at least one</summary>
    public static string? DeleteRefusal(CutsceneFile file) => file.Mqb is { Cuts.Count: >= 2 } ? null : "CUTSCENE_Cut_Delete_Last_Refused";

    /// <summary>
    /// Takes a cut out, with the resources no other cut uses (the other events' and resources' indices follow), and
    /// the Havok files of its own folders, whose binder entries go on save
    /// </summary>
    public static CutsceneEditAction? Delete(ProjectEntry project, CutsceneFile file, MQB.Cut cut)
    {
        if (Refused(DeleteRefusal(file), cut.Name))
            return null;

        var mqb = file.Mqb!;
        var index = mqb.Cuts.IndexOf(cut);
        if (index < 0)
            return null;

        var others = mqb.Cuts.Where(c => c != cut).SelectMany(c => UsedResources(mqb, c)).ToHashSet();
        var unused = UsedResources(mqb, cut).Where(i => !others.Contains(i)).ToHashSet();

        // Nor one a kept resource is parented to
        for (var changed = true; changed;)
        {
            changed = false;
            for (var i = 0; i < mqb.Resources.Count; i++)
                if (!unused.Contains(i) && unused.Remove(mqb.Resources[i].ParentIndex))
                    changed = true;
        }

        var folders = HavokFolders(mqb, unused);
        folders.ExceptWith(HavokFolders(mqb, others));
        if (file.HasCutFolder(cut.Name) && !HavokFolders(mqb, others).Contains(cut.Name))
            folders.Add(cut.Name);

        // As they are, to put back
        var resources = mqb.Resources.ToList();
        var parents = resources.ToDictionary(r => r, r => r.ParentIndex);
        var events = mqb.Cuts.SelectMany(c => c.Timelines).SelectMany(t => t.Events).ToDictionary(e => e, e => e.ResourceIndex);
        var havok = new Dictionary<string, CutsceneFile.CutHavok>();

        // The index each kept resource moves to
        var map = new Dictionary<int, int>();
        for (int i = 0, n = 0; i < resources.Count; i++)
            if (!unused.Contains(i))
                map[i] = n++;

        return new CutsceneEditAction(file,
            () =>
            {
                mqb.Cuts.Remove(cut);

                mqb.Resources = resources.Where((_, i) => !unused.Contains(i)).ToList();
                foreach (var r in mqb.Resources)
                    r.ParentIndex = map.GetValueOrDefault(parents[r], -1);
                foreach (var (e, ri) in events)
                    if (map.TryGetValue(ri, out var moved))
                        e.ResourceIndex = moved;

                foreach (var folder in folders)
                {
                    if (!havok.ContainsKey(folder))
                        havok[folder] = file.TakeCut(project, folder);
                    file.RemoveCut(folder);
                }
            },
            () =>
            {
                mqb.Resources = resources.ToList();
                foreach (var (r, parent) in parents)
                    r.ParentIndex = parent;
                foreach (var (e, ri) in events)
                    e.ResourceIndex = ri;

                mqb.Cuts.Insert(Math.Min(index, mqb.Cuts.Count), cut);

                foreach (var (folder, h) in havok)
                    file.PutCut(project, folder, h);
            });
    }

    /// <summary>A copy of a cut, its resources and its Havok files, as they are now</summary>
    public static CutsceneCutClipboard Copy(ProjectEntry project, CutsceneFile file, MQB.Cut cut)
    {
        var mqb = file.Mqb!;
        var used = UsedResources(mqb, cut);

        var resources = new SortedDictionary<int, (MQB.Resource, MQB.Resource)>();
        foreach (var i in used)
            resources[i] = (mqb.Resources[i], CutsceneMqbOps.Clone(mqb.Resources[i]));

        var havok = new Dictionary<string, CutsceneFile.CutHavok>(StringComparer.OrdinalIgnoreCase);
        foreach (var folder in HavokFolders(mqb, used))
            havok[folder] = file.TakeCut(project, folder);

        return new CutsceneCutClipboard
        {
            SourceFile = file,
            SourceName = file.Name,
            Framerate = mqb.Framerate,
            Cut = Clone(cut),
            Resources = resources,
            Havok = havok
        };
    }

    /// <summary>
    /// A copied cut, at an index of a cutscene, under a name. Its actors, cameras, offsets and HKTs (and what hangs from
    /// them) are new resources, their HKTs and Havok files in the new cut's folder; in the cutscene it was copied from,
    /// its other resources are the ones it used, elsewhere copies. Its events take new IDs.
    /// </summary>
    public static CutsceneEditAction? Paste(ProjectEntry project, CutsceneFile file, int index, string name, CutsceneCutClipboard clip)
    {
        if (Refused(NameRefusal(file, name), name))
            return null;

        var mqb = file.Mqb!;
        if (Math.Abs(mqb.Framerate - clip.Framerate) > 0.01f)
            Smithbox.Log(typeof(CutsceneCutOps), LOC.Get("CUTSCENE_Cut_Paste_Framerate", clip.Framerate, mqb.Framerate, file.Name),
                Microsoft.Extensions.Logging.LogLevel.Warning);

        var sameFile = clip.SourceFile == file;

        // The source's resources, in this file: the same where they can be shared, else new ones at the end
        var map = new Dictionary<int, int>();
        var added = new List<(int Source, MQB.Resource Resource)>();
        foreach (var (source, (original, copy)) in clip.Resources)
        {
            var existing = sameFile ? mqb.Resources.IndexOf(original) : -1;
            if (existing >= 0 && !IsPerCut(mqb, existing))
            {
                map[source] = existing;
                continue;
            }

            map[source] = mqb.Resources.Count + added.Count;
            added.Add((source, CutsceneMqbOps.Clone(copy)));
        }

        foreach (var (source, r) in added)
        {
            var parent = clip.Resources[source].Copy.ParentIndex;
            // A used resource's parents are used too, so the parent is in the map
            r.ParentIndex = map.TryGetValue(parent, out var p) ? p : -1;

            if (r.Path != null)
            {
                var folder = CutsceneFile.CutFolderOf(r.Path);
                if (r.Type == CutsceneResourceTypes.Animation && folder != null)
                    r.Path = CutsceneFile.ReplaceFolder(r.Path, folder, name);
                r.Path = CutsceneFile.ReplaceFolder(r.Path, clip.SourceName, file.Name);
            }
        }

        var cut = Clone(clip.Cut);
        cut.Name = name;
        var nextId = CutsceneMqbOps.NextEventId(mqb);
        foreach (var e in cut.Timelines.SelectMany(t => t.Events))
        {
            e.ID = nextId++;
            e.ResourceIndex = map.TryGetValue(e.ResourceIndex, out var ri) ? ri : -1;
        }

        index = Math.Clamp(index, 0, mqb.Cuts.Count);
        var resources = added.Select(a => a.Resource).ToList();

        return new CutsceneEditAction(file,
            () =>
            {
                mqb.Resources.AddRange(resources);
                mqb.Cuts.Insert(index, cut);
                foreach (var h in clip.Havok.Values)
                    file.PutCut(project, name, h);
            },
            () =>
            {
                mqb.Cuts.Remove(cut);
                foreach (var r in resources)
                    mqb.Resources.Remove(r);
                if (clip.Havok.Count > 0)
                    file.RemoveCut(name);
            });
    }

    private static bool Refused(string? refusal, string name)
    {
        if (refusal == null)
            return false;

        Smithbox.Log(typeof(CutsceneCutOps), LOC.Get(refusal, name), Microsoft.Extensions.Logging.LogLevel.Warning);
        return true;
    }
}

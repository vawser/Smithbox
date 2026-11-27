using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// The editor side of Gparam Merge's inputs (Claude/Documentation/Gparam.md §7.2, §7.3): this project's files
/// as analysis targets, and the sources that need the editor (another project, a transfer export). The merge itself
/// (GparamMerge, GparamMergeSource, GparamMergeAnalysis) has no editor dependencies.
/// </summary>
public static class GparamMergeSources
{
    #region Targets

    /// <summary>
    /// This project's GPARAMs as targets. Call on the UI thread: an open file's mine is cloned here (its unsaved
    /// edits included, and the worker never reads a live object); an unopened one is read on the worker.
    /// </summary>
    public static List<GparamMergeTarget> BuildTargets(ProjectEntry project)
    {
        var data = project.Handler.GparamData;
        var primary = data.PrimaryBank;
        var vanilla = data.VanillaBank;
        var targets = new List<GparamMergeTarget>();

        foreach (var (entry, wrapper) in primary.Entries)
        {
            var key = new GparamMergeFileKey(entry.Filename, entry.Extension);

            Func<(GPARAM, GparamMergeProblem)> readMine;

            if (wrapper.IsLoaded())
            {
                var snapshot = wrapper.Gparam.Clone();
                readMine = () => (snapshot, null);
            }
            else
            {
                readMine = () => ReadFromBank(primary, entry, key);
            }

            targets.Add(new GparamMergeTarget
            {
                Key = key,
                Tag = entry,
                ReadMine = readMine,
                ReadVanilla = () => ReadVanilla(vanilla, entry, key)
            });
        }

        return targets;
    }

    private static (GPARAM, GparamMergeProblem) ReadFromBank(GparamBank bank, FileDictionaryEntry entry, GparamMergeFileKey key)
    {
        try
        {
            if (!bank.TryReadGraphicsParam(entry, out var gparam))
                return (null, new GparamMergeProblem("Read_Failed", entry.Path));

            return gparam == null
                ? (null, new GparamMergeProblem("Not_In_Binder", entry.Path, key.Filename))
                : (gparam, null);
        }
        catch (Exception e)
        {
            return (null, new GparamMergeProblem("Read_Failed", entry.Path, e.Message));
        }
    }

    /// <summary>The game's copy, or none (no problem) when the game has no such file</summary>
    private static (GPARAM, GparamMergeProblem) ReadVanilla(GparamBank vanilla, FileDictionaryEntry entry, GparamMergeFileKey key)
    {
        if (vanilla == null)
            return (null, null);

        var vanillaEntry = vanilla.Entries.Keys.FirstOrDefault(e =>
            string.Equals(e.Filename, entry.Filename, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(e.Extension, entry.Extension, StringComparison.OrdinalIgnoreCase));

        // A file the project added: the vanilla bank lists it too (same locator), but the game doesn't have it.
        // Checked first so reading it doesn't log a missing-file error.
        if (vanillaEntry == null || !vanilla.TargetFS.FileExists(vanillaEntry.Path))
            return (null, null);

        var (gparam, problem) = ReadFromBank(vanilla, vanillaEntry, key);

        // A binder the game has without this GPARAM in it: no vanilla copy either
        return problem?.Code == "Not_In_Binder" ? (null, null) : (gparam, problem);
    }

    #endregion

    #region Sources

    /// <summary>
    /// Another project of the same game (D1: loaded as an auxiliary project, VFS and locator only). Only the files in
    /// its own folder are read: the rest are the game's, with nothing to merge. Run off the UI thread.
    /// </summary>
    public static async Task<GparamMergeSource> FromProject(ProjectEntry current, ProjectEntry other)
    {
        var name = other.Descriptor.ProjectName;

        if (other.Descriptor.ProjectType != current.Descriptor.ProjectType)
            return GparamMergeSource.Failed(GparamMergeSourceType.Project, name, "Other_Game", other.Descriptor.ProjectType.ToString());

        await Smithbox.Orchestrator.LoadAuxiliaryProject(other, ProjectInitType.GparamEditorOnly, false);

        if (other.VFS == null)
            return GparamMergeSource.Failed(GparamMergeSourceType.Project, name, "Project_Not_Loaded");

        if (other.VFS.ProjectFS is EmptyVirtualFileSystem)
            return GparamMergeSource.Failed(GparamMergeSourceType.Project, name, "No_Project_Folder", other.Descriptor.ProjectPath);

        return GparamMergeSource.FromFileSystem(GparamMergeSourceType.Project, name, other.VFS.ProjectFS);
    }

    /// <summary>
    /// A mod folder: the chosen folder, its parent or grandparent, whichever holds this project's GPARAM paths
    /// (else any GPARAM), read through a read-only file system the source owns.
    /// </summary>
    public static GparamMergeSource FromModFolder(ProjectEntry project, string folder)
    {
        var expected = project.Locator.GparamFiles.Entries.Select(e => e.Path).ToList();
        var root = GparamMergeSource.ResolveModRoot(folder, expected);

        if (root == null)
            return GparamMergeSource.Failed(GparamMergeSourceType.ModFolder, folder ?? "", "No_Gparams");

        return GparamMergeSource.FromFileSystem(GparamMergeSourceType.ModFolder, root,
            new RealVirtualFileSystem(root, true), ownsFileSystem: true);
    }

    /// <summary>
    /// A Data Transfer export (<see cref="GparamTransferPayload"/>). A whole GPARAM is matched by the file it was
    /// exported from (else the selected file). Groups, fields and values become their side of the file they were
    /// exported from if this project has it, else the selected one; fields go into their group (else the selected
    /// group), values into their field (else the selected field). Partial: never reports removals.
    /// Call on the UI thread (reads the selection and the open file).
    /// </summary>
    public static GparamMergeSource FromTransferFile(ProjectEntry project, GparamSelection selection, string path)
    {
        const GparamMergeSourceType type = GparamMergeSourceType.TransferExport;

        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (Exception e)
        {
            return GparamMergeSource.Failed(type, path, "Read_Failed", e.Message);
        }

        var payload = GparamTransferPayload.Parse(json, out var parseError);
        if (payload == null)
            return GparamMergeSource.Failed(type, path, "Not_Transfer", parseError);

        var problems = new List<GparamMergeProblem>();
        var game = project.Descriptor.ProjectType.ToString();

        if (!string.IsNullOrEmpty(payload.Game) && payload.Game != game)
            problems.Add(new GparamMergeProblem("Other_Game", path, payload.Game));

        var picked = GparamTransferTarget.FromSelection(selection);
        var exportedFile = payload.Source?.File;

        GparamMergeSource source;

        if (payload.Kind == GparamTransferKind.Gparam)
        {
            GPARAM gparam;
            try
            {
                gparam = payload.GetGparam();
            }
            catch (Exception e)
            {
                return GparamMergeSource.Failed(type, path, "Not_Transfer", e.Message);
            }

            if (gparam == null)
                return GparamMergeSource.Failed(type, path, "Not_Transfer");

            GparamMergeFileKey key;
            if (!string.IsNullOrEmpty(exportedFile))
            {
                key = new GparamMergeFileKey(exportedFile, null);
            }
            else if (picked.FileEntry != null)
            {
                key = new GparamMergeFileKey(picked.FileEntry.Filename, picked.FileEntry.Extension);
                problems.Add(new GparamMergeProblem("Using_Selected_File", picked.FileEntry.Filename));
            }
            else
            {
                return GparamMergeSource.Failed(type, path, "No_Target_File");
            }

            source = GparamMergeSource.FromGparam(type, path, key, gparam, whole: true);
        }
        else
        {
            source = FromPartialPayload(project, payload, picked, path, problems);
        }

        source.Problems.AddRange(problems);
        return source;
    }

    private static GparamMergeSource FromPartialPayload(ProjectEntry project, GparamTransferPayload payload,
        GparamTransferTarget picked, string path, List<GparamMergeProblem> problems)
    {
        const GparamMergeSourceType type = GparamMergeSourceType.TransferExport;
        var bank = project.Handler.GparamData.PrimaryBank;

        // The file: the one it was exported from, if this project has exactly one of that name, else the selected one
        var exported = payload.Source?.File;
        var byName = string.IsNullOrEmpty(exported)
            ? new List<FileDictionaryEntry>()
            : bank.Entries.Keys.Where(e => string.Equals(e.Filename, exported, StringComparison.OrdinalIgnoreCase)).Take(2).ToList();

        var entry = byName.Count == 1 ? byName[0] : picked.FileEntry;
        if (entry == null)
            return GparamMergeSource.Failed(type, path, "No_Target_File");

        if (byName.Count != 1)
            problems.Add(new GparamMergeProblem("Using_Selected_File", entry.Filename));

        var key = new GparamMergeFileKey(entry.Filename, entry.Extension);

        var (mine, mineProblem) = bank.Entries.TryGetValue(entry, out var wrapper) && wrapper.IsLoaded()
            ? (wrapper.Gparam, null)
            : ReadFromBank(bank, entry, key);

        if (mine == null)
            return GparamMergeSource.Failed(type, path, mineProblem?.Code ?? "Read_Failed", mineProblem?.Detail ?? "");

        // The group / field: where it was exported from, if the file has it, else the selection
        string Pick(string exportedKey, string selectedKey, Func<string, bool> exists)
        {
            if (!string.IsNullOrEmpty(exportedKey) && exists(exportedKey))
                return exportedKey;

            if (selectedKey != null)
                problems.Add(new GparamMergeProblem("Using_Selected_Entry", selectedKey));

            return selectedKey;
        }

        GPARAM theirs;
        GparamMergeProblem overlayProblem = null;

        try
        {
            switch (payload.Kind)
            {
                case GparamTransferKind.Group:
                    theirs = GparamMergeSource.OverlayGroups(mine, payload.GetGroups());
                    break;

                case GparamTransferKind.Field:
                {
                    var groupKey = Pick(payload.Source?.Group, picked.Group?.Key,
                        k => GparamMerge.FindGroup(mine, k) != null);
                    theirs = GparamMergeSource.OverlayFields(mine, groupKey, payload.GetFields(), out overlayProblem);
                    break;
                }

                default:
                {
                    var groupKey = Pick(payload.Source?.Group, picked.Group?.Key,
                        k => GparamMerge.FindGroup(mine, k) != null);
                    var group = GparamMerge.FindGroup(mine, groupKey);
                    var fieldKey = Pick(payload.Source?.Field, picked.Field?.Key,
                        k => GparamMerge.FindField(group, k) != null);
                    theirs = GparamMergeSource.OverlayValues(mine, groupKey, fieldKey, payload.GetValues(), out overlayProblem);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            return GparamMergeSource.Failed(type, path, "Not_Transfer", e.Message);
        }

        if (theirs == null)
            return GparamMergeSource.Failed(type, path, overlayProblem?.Code ?? "Not_Transfer", overlayProblem?.Subject ?? "");

        return GparamMergeSource.FromGparam(type, path, key, theirs, whole: false);
    }

    #endregion
}

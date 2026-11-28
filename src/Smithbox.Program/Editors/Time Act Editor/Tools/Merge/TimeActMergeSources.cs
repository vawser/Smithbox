using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// The editor side of Time Act Merge's inputs (Claude/Documentation/Time Act.md §5.2): this project's
/// sources as analysis targets, and the other side's sources. Every side is read through
/// <see cref="TimeActContainerWrapper.ReadTimeActs"/> over its own file system, so theirs are read exactly as this
/// project's are (BND3 / BND4 / BXF, an asset's nested anibnd, AC6 action time acts). The merge itself
/// (TimeActMerge, TimeActMergeSource, TimeActMergeAnalysis) has no editor dependencies.
/// </summary>
public static class TimeActMergeSources
{
    /// <summary>A problem as the review and the log show it: <c>TAE_Merge_Problem_{Code}</c>, {0} subject, {1} detail</summary>
    public static string Describe(TimeActMergeProblem problem) =>
        LOC.Get($"TAE_Merge_Problem_{problem.Code}", problem.Subject, problem.Detail ?? "");

    #region Targets

    /// <summary>
    /// This project's sources as targets. Call on the UI thread: an open source's time acts are copied here (their
    /// unsaved edits written out, so the worker never reads a live object); an unopened one is read on the worker.
    /// </summary>
    public static List<TimeActMergeTarget> BuildTargets(ProjectEntry project)
    {
        var bank = project.Handler.TimeActData?.PrimaryBank;
        var targets = new List<TimeActMergeTarget>();

        if (bank == null)
            return targets;

        var vanillaFS = project.VFS.VanillaFS;

        foreach (var source in bank.GetAllSources())
        {
            Func<(List<TimeActMergeTimeAct>, TimeActMergeProblem)> readMine;
            var open = source.TimeActs.Count > 0;

            if (open)
            {
                var copies = source.TimeActs.Select(ToMergeTimeAct).ToList();
                readMine = () => (copies, null);
            }
            else
            {
                readMine = () => Read(source, source.SourcePath);
            }

            // The imports as they are now: the worker never reads the live list
            var pending = source.PendingAnimationFiles.ToDictionary(e => e.Key, e => e.Value);
            var hasVanilla = vanillaFS != null && vanillaFS.FileExists(source.SourcePath);

            targets.Add(new TimeActMergeTarget
            {
                Key = TimeActMergeSourceKey.Of(source.SourcePath),
                Path = source.SourcePath,
                DisplayName = source.DisplayName,
                Tag = source,
                ReadMine = readMine,
                ReadVanilla = () => ReadVanilla(source, vanillaFS),
                MineReadForMerge = !open,
                PlanClips = file => TimeActMergeClips.Plan(source, pending, file.Tag as TimeActContainerWrapper,
                    hasVanilla ? source.CreateVanillaCopy() : null, source.DisplayName)
            });
        }

        return targets;
    }

    /// <summary>
    /// A time act as the merge takes it: its bytes as they are now (an edited one written out, so the copy holds
    /// its unsaved changes) and the editor's entry as its tag
    /// </summary>
    private static TimeActMergeTimeAct ToMergeTimeAct(TimeActEntry entry) => new()
    {
        BinderID = entry.BinderID,
        Name = entry.Name,
        BinderFileName = entry.BinderFileName,
        BinderFlags = entry.BinderFlags,
        CompressionType = entry.CompressionType,
        Bytes = entry.GetSharedBytes(),
        Tag = entry
    };

    private static (List<TimeActMergeTimeAct>, TimeActMergeProblem) Read(TimeActContainerWrapper wrapper, string path)
    {
        try
        {
            var entries = wrapper.ReadTimeActs();

            if (entries.Count == 0)
                return (null, new TimeActMergeProblem("No_Time_Acts", path));

            return (entries.Select(ToMergeTimeAct).ToList(), null);
        }
        catch (Exception e)
        {
            return (null, new TimeActMergeProblem("Read_Failed", path, e.Message));
        }
    }

    /// <summary>The game's copy, or none (no problem) when the game has no such source</summary>
    private static (List<TimeActMergeTimeAct>, TimeActMergeProblem) ReadVanilla(TimeActContainerWrapper source,
        VirtualFileSystem vanillaFS)
    {
        // A source the project added: checked first, so reading it logs no missing-file error
        if (vanillaFS == null || !vanillaFS.FileExists(source.SourcePath))
            return (null, null);

        var (timeActs, problem) = Read(source.CreateVanillaCopy(), source.SourcePath);

        return problem == null ? (timeActs, null)
            : (null, new TimeActMergeProblem("Vanilla_Unreadable", source.DisplayName, problem.Detail));
    }

    #endregion

    #region Sources

    /// <summary>
    /// Another project of the same game, loaded as an auxiliary project (VFS and locator only). Only the sources in its
    /// own folder are read: the rest are the game's, with nothing to merge. Run off the UI thread.
    /// </summary>
    public static async Task<TimeActMergeSource> FromProject(ProjectEntry current, ProjectEntry other,
        IReadOnlyList<TimeActMergeTarget> targets)
    {
        var name = other.Descriptor.ProjectName;

        if (other.Descriptor.ProjectType != current.Descriptor.ProjectType)
            return TimeActMergeSource.Failed(TimeActMergeSourceType.Project, name, "Other_Game",
                other.Descriptor.ProjectType.ToString());

        await Smithbox.Orchestrator.LoadAuxiliaryProject(other, ProjectInitType.TimeActEditorOnly, false);

        if (other.VFS == null)
            return TimeActMergeSource.Failed(TimeActMergeSourceType.Project, name, "Project_Not_Loaded");

        if (other.VFS.ProjectFS is EmptyVirtualFileSystem)
            return TimeActMergeSource.Failed(TimeActMergeSourceType.Project, name, "No_Project_Folder",
                other.Descriptor.ProjectPath);

        return FromFileSystem(current, TimeActMergeSourceType.Project, name, other.VFS.ProjectFS, targets,
            ownsFileSystem: false);
    }

    /// <summary>
    /// A mod folder: the chosen folder, its parent or grandparent, whichever holds this project's source paths (else
    /// a character anibnd), read through a read-only file system the source owns
    /// </summary>
    public static TimeActMergeSource FromModFolder(ProjectEntry project, string folder, IReadOnlyList<TimeActMergeTarget> targets)
    {
        var root = TimeActMergeSource.ResolveModRoot(folder, targets.Select(t => t.Path));

        if (root == null)
            return TimeActMergeSource.Failed(TimeActMergeSourceType.ModFolder, folder ?? "", "No_Sources");

        return FromFileSystem(project, TimeActMergeSourceType.ModFolder, root, new RealVirtualFileSystem(root, true),
            targets, ownsFileSystem: true);
    }

    /// <summary>
    /// Every target the file system has at its path (or with the other compression extension), whole; and its
    /// character anibnds this project has no source for, unmatched (offered for Add Source)
    /// </summary>
    private static TimeActMergeSource FromFileSystem(ProjectEntry project, TimeActMergeSourceType type, string name,
        VirtualFileSystem fs, IReadOnlyList<TimeActMergeTarget> targets, bool ownsFileSystem)
    {
        var source = new TimeActMergeSource { Type = type, Name = name, FileSystem = fs, OwnsFileSystem = ownsFileSystem };
        var seen = new HashSet<string>();

        foreach (var target in targets)
        {
            if (!seen.Add(target.Key))
                continue;

            var path = FindPath(fs, target.Path);
            if (path == null)
                continue;

            source.Files.Add(MakeFile(project, fs, target.Key, path, Path.GetFileName(path)));
        }

        foreach (var path in TimeActMergeSource.FindCharacterAnibnds(fs))
        {
            var key = TimeActMergeSourceKey.Of(path);

            if (seen.Add(key))
                source.Files.Add(MakeFile(project, fs, key, path, Path.GetFileName(path)));
        }

        if (source.Files.Count == 0)
            source.Problems.Add(new TimeActMergeProblem("No_Sources", name));

        return source;
    }

    private static TimeActMergeSourceFile MakeFile(ProjectEntry project, VirtualFileSystem fs, string key, string path,
        string fileName)
    {
        var wrapper = TimeActContainerWrapper.ForFile(project, SourceName(fileName), path, fs);

        return new TimeActMergeSourceFile
        {
            Key = key,
            Path = path,
            Read = () => Read(wrapper, path),
            Tag = wrapper
        };
    }

    /// <summary>The path as the file system has it: as given, or with the compression extension added or dropped</summary>
    private static string FindPath(VirtualFileSystem fs, string path)
    {
        if (fs.FileExists(path))
            return path;

        var other = path.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase) ? path[..^4] : path + ".dcx";

        return fs.FileExists(other) ? other : null;
    }

    /// <summary>c3000.anibnd.dcx → c3000</summary>
    private static string SourceName(string fileName) => fileName.Split('.')[0];

    /// <summary>
    /// One loose file. An anibnd or container is matched to this project's source of that file name, whole. A lone
    /// .tae merges into the selected source, as the time act of the same binder file name; it is whole for that time
    /// act alone.
    /// </summary>
    public static TimeActMergeSource FromLooseFile(ProjectEntry project, string path, TimeActContainerWrapper selected)
    {
        const TimeActMergeSourceType type = TimeActMergeSourceType.LooseFile;

        if (!File.Exists(path))
            return TimeActMergeSource.Failed(type, path ?? "", "Not_Found");

        var folder = Path.GetDirectoryName(Path.GetFullPath(path));
        var fileName = Path.GetFileName(path);
        var fs = new RealVirtualFileSystem(folder, true);
        var source = new TimeActMergeSource { Type = type, Name = path, FileSystem = fs, OwnsFileSystem = true };

        var key = TimeActMergeSourceKey.Of(fileName);

        if (!TimeActMergeSourceKey.FileName(key).EndsWith(".tae"))
        {
            var file = MakeFile(project, fs, TimeActMergeSourceKey.FileName(key), "/" + fileName, fileName);

            source.Files.Add(new TimeActMergeSourceFile
            {
                Key = file.Key,
                Path = file.Path,
                ByFileName = true,
                Read = file.Read,
                Tag = file.Tag
            });

            return source;
        }

        if (selected == null)
        {
            source.Dispose();
            return TimeActMergeSource.Failed(type, path, "No_Selected_Source");
        }

        if (selected.TimeActs.Count == 0)
        {
            source.Dispose();
            return TimeActMergeSource.Failed(type, path, "Selected_Source_Not_Open", selected.DisplayName);
        }

        // The time act whose binder file has this name
        var taeName = TimeActMergeSourceKey.FileName(key);
        var entry = selected.TimeActs.FirstOrDefault(e =>
            string.Equals(Path.GetFileName(e.BinderFileName ?? ""), taeName, StringComparison.OrdinalIgnoreCase));

        if (entry == null)
        {
            source.Dispose();
            return TimeActMergeSource.Failed(type, path, "No_Matching_Time_Act", selected.DisplayName);
        }

        source.Problems.Add(new TimeActMergeProblem("Using_Selected_Source", selected.DisplayName, entry.Name));

        source.Files.Add(new TimeActMergeSourceFile
        {
            Key = taeName,
            Path = path,
            OnlyBinderID = entry.BinderID,
            TargetKey = TimeActMergeSourceKey.Of(selected.SourcePath),
            Read = () => ReadLooseTimeAct(path, entry)
        });

        return source;
    }

    /// <summary>A .tae on its own, DCX or not, as the time act of the entry it merges into</summary>
    private static (List<TimeActMergeTimeAct>, TimeActMergeProblem) ReadLooseTimeAct(string path, TimeActEntry like)
    {
        try
        {
            var bytes = File.ReadAllBytes(path);

            if (DCX.Is(bytes))
                bytes = DCX.Decompress(bytes).ToArray();

            if (bytes.Length < 4 || bytes[0] != 'T' || bytes[1] != 'A' || bytes[2] != 'E')
                return (null, new TimeActMergeProblem("Not_Time_Act", path));

            var timeAct = new TimeActMergeTimeAct
            {
                BinderID = like.BinderID,
                Name = like.Name,
                BinderFileName = like.BinderFileName,
                BinderFlags = like.BinderFlags,
                CompressionType = like.CompressionType,
                Bytes = bytes
            };

            if (timeAct.ReadFailed)
                return (null, new TimeActMergeProblem("Read_Failed", path));

            return (new List<TimeActMergeTimeAct> { timeAct }, null);
        }
        catch (Exception e)
        {
            return (null, new TimeActMergeProblem("Read_Failed", path, e.Message));
        }
    }

    #endregion
}

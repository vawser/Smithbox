using Andre.IO.VFS;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>What an Apply or Add Source did</summary>
public class TimeActMergeApplyReport
{
    public int Applied;
    public int Stale;
    public int Unresolved;
    public int Sources;

    /// <summary>Sources added to the project (Add Source)</summary>
    public int Added;

    public List<string> StaleItems { get; } = new();
    public List<TimeActMergeProblem> Problems { get; } = new();
}

/// <summary>
/// Turns an analysis into one undo step in the view's history (D4: as Mass Edit's All Sources), and adds the sources
/// only the other side has (Claude/Documentation/Time Act.md §5.2). Call on the UI thread.
/// </summary>
public static class TimeActMergeApplier
{
    /// <summary>
    /// Applies every ticked source's chosen items as one compound action named after the source ("Time Act Merge
    /// from X"): per time act a <see cref="TimeActMergeAction"/>, whole time acts added or removed with
    /// <see cref="TimeActAddEntryAction"/>, clips queued as imports. An unopened source first takes the list read for
    /// the analysis (saving writes the list as the whole binder, so it is never a partial one). Unresolved conflicts
    /// keep mine. Nothing happens to a source edited since in a way the items can't apply to: those items are stale.
    /// </summary>
    public static TimeActMergeApplyReport Apply(TimeActEditorView view, TimeActMergeAnalysisResult analysis)
    {
        var report = new TimeActMergeApplyReport();
        var actions = new List<ViewportAction>();
        var merges = new List<TimeActMergeAction>();

        foreach (var result in analysis.Sources.Where(e => e.Included))
        {
            var source = (TimeActContainerWrapper)result.Target.Tag;
            var items = result.Plan.Items;

            report.Unresolved += items.Count(e => e.Included && e.Kind == TimeActMergeKind.Conflict &&
                e.Choice == TimeActMergeChoice.Unresolved);

            var chosen = TimeActMerge.ChosenItems(result.Plan);
            if (chosen.Count == 0)
                continue;

            // An unopened source takes the list it was analysed with; one opened since keeps its own
            if (result.Target.MineReadForMerge && source.TimeActs.Count == 0)
                source.TimeActs = result.Mine.Select(e => (TimeActEntry)e.Tag).ToList();

            if (source.TimeActs.Count == 0)
            {
                Stale(report, chosen);
                continue;
            }

            report.Sources++;

            foreach (var item in chosen.Where(e => e.Level == TimeActMergeLevel.TimeAct))
                AddTimeActAction(view, source, item, actions, report);

            foreach (var group in chosen.Where(e => e.Level is not (TimeActMergeLevel.TimeAct or TimeActMergeLevel.Clip))
                         .GroupBy(e => e.BinderID))
            {
                var entry = source.TimeActs.FirstOrDefault(e => e.BinderID == group.Key);

                if (entry == null)
                {
                    Stale(report, group);
                    continue;
                }

                var merge = new TimeActMergeAction(view, source, entry, group);
                merges.Add(merge);
                actions.Add(merge);
            }

            foreach (var item in chosen.Where(e => e.Level == TimeActMergeLevel.Clip))
                AddClipAction(source, item, actions, report);
        }

        if (actions.Count > 0)
        {
            view.ViewportActionManager.ExecuteAction(new TimeActCompoundAction(actions,
                LOC.Get("TAE_Merge_Undo_Name", analysis.SourceName)));
        }

        foreach (var merge in merges)
        {
            report.Applied += merge.LastResult?.Applied ?? 0;
            report.Stale += merge.LastResult?.Stale ?? 0;

            if (merge.LastResult != null)
                report.StaleItems.AddRange(merge.LastResult.StaleItems);
        }

        Log(report, analysis.SourceName);
        return report;
    }

    private static void Stale(TimeActMergeApplyReport report, IEnumerable<TimeActMergeItem> items)
    {
        foreach (var item in items)
        {
            report.Stale++;
            report.StaleItems.Add(item.Path);
        }
    }

    /// <summary>A whole time act: theirs added (or put back over my deletion), or mine removed as they removed it</summary>
    private static void AddTimeActAction(TimeActEditorView view, TimeActContainerWrapper source, TimeActMergeItem item,
        List<ViewportAction> actions, TimeActMergeApplyReport report)
    {
        var live = source.TimeActs.FirstOrDefault(e => e.BinderID == item.BinderID);

        if (item.Theirs is TimeActMergeTimeAct theirs)
        {
            // Their entry, read for the analysis and listed nowhere, becomes mine: written as new when saved
            if (live != null || theirs.Tag is not TimeActEntry entry)
            {
                Stale(report, [item]);
                return;
            }

            entry.IsDirty = true;
            actions.Add(new TimeActAddEntryAction(view, source, entry));
            report.Applied++;
            return;
        }

        // Removed by them: only while mine is still as analysed
        var analysed = item.Mine as TimeActMergeTimeAct;
        var current = live == null ? null : new TimeActMergeTimeAct { Bytes = live.GetSharedBytes() };

        if (current == null || analysed == null || !TimeActMerge.SameTimeAct(current, analysed))
        {
            Stale(report, [item]);
            return;
        }

        actions.Add(new TimeActAddEntryAction(view, source, live, isDelete: true));
        report.Applied++;
    }

    /// <summary>Their clip, queued as an import, while mine's import of it (or none) is still as analysed</summary>
    private static void AddClipAction(TimeActContainerWrapper source, TimeActMergeItem item, List<ViewportAction> actions,
        TimeActMergeApplyReport report)
    {
        var clip = (TimeActMergeClip)item.Theirs;
        source.PendingAnimationFiles.TryGetValue(clip.ID, out var pending);

        if (!ReferenceEquals(pending, item.Mine))
        {
            Stale(report, [item]);
            return;
        }

        actions.Add(new TimeActImportAnimationFileAction(source,
            new TimeActPendingAnimationFile(clip.ID, clip.Bytes, clip.BinderPaths)));
        report.Applied++;
    }

    private static void Log(TimeActMergeApplyReport report, string sourceName)
    {
        Smithbox.Log(typeof(TimeActMergeApplier), LOC.Get("TAE_Merge_Applied", sourceName, report.Applied, report.Sources));

        if (report.Stale > 0)
        {
            Smithbox.LogWarning(typeof(TimeActMergeApplier), LOC.Get("TAE_Merge_Stale", report.Stale,
                string.Join(", ", report.StaleItems.Take(10))));
        }

        if (report.Unresolved > 0)
            Smithbox.LogWarning(typeof(TimeActMergeApplier), LOC.Get("TAE_Merge_Unresolved", report.Unresolved));

        foreach (var problem in report.Problems)
            Smithbox.LogWarning(typeof(TimeActMergeApplier), TimeActMergeSources.Describe(problem));
    }

    #region Add Source

    /// <summary>
    /// Writes character anibnds only the other side has into this project, as stored, with their split anibnds (which
    /// hold their clips), at the paths they have there; lists them as the locator does and in the source list. Refuses
    /// a source this project already has, or a path its folder already holds. Not undoable: the panel confirms first.
    /// </summary>
    public static TimeActMergeApplyReport AddSources(ProjectEntry project, TimeActMergeSource source,
        IEnumerable<TimeActMergeSourceFile> files)
    {
        var report = new TimeActMergeApplyReport();
        var projectFs = project.VFS.ProjectFS;
        var bank = project.Handler.TimeActData?.PrimaryBank;

        if (projectFs is EmptyVirtualFileSystem || bank == null || source.FileSystem == null)
        {
            report.Problems.Add(new TimeActMergeProblem("No_Project_Folder", project.Descriptor.ProjectName));
            Log(report, source.Name);
            return report;
        }

        var listed = bank.GetAllSources().Select(e => TimeActMergeSourceKey.Of(e.SourcePath)).ToHashSet();

        foreach (var file in files)
        {
            if (file.ByFileName || file.OnlyBinderID != null || !file.Path.StartsWith("/chr/", StringComparison.OrdinalIgnoreCase))
            {
                report.Problems.Add(new TimeActMergeProblem("Cannot_Add", file.Path));
                continue;
            }

            if (listed.Contains(TimeActMergeSourceKey.Of(file.Path)) || project.VFS.FS.FileExists(file.Path))
            {
                report.Problems.Add(new TimeActMergeProblem("Exists", file.Path));
                continue;
            }

            var name = Path.GetFileName(file.Path).Split('.')[0];
            var paths = AnimationPackage.FindCharacterBinders(source.FileSystem, file.Path, name).ToList();

            if (!paths.Contains(file.Path, StringComparer.OrdinalIgnoreCase))
                paths.Insert(0, file.Path);

            var written = new List<string>();

            foreach (var path in paths)
            {
                // A split anibnd the project somehow has already is left as it is
                if (project.VFS.FS.FileExists(path))
                    continue;

                try
                {
                    if (source.FileSystem.ReadFile(path) is not { } data)
                        throw new IOException(path);

                    projectFs.WriteFile(path, data.ToArray());
                    written.Add(path);
                }
                catch (Exception e)
                {
                    report.Problems.Add(new TimeActMergeProblem("Write_Failed", path, e.Message));
                }
            }

            if (!written.Contains(file.Path, StringComparer.OrdinalIgnoreCase))
                continue;

            FileDictionaryEntry mainEntry = null;

            foreach (var path in written)
            {
                var entry = Register(project, path);

                if (path.Equals(file.Path, StringComparison.OrdinalIgnoreCase))
                    mainEntry = entry;
            }

            if (mainEntry != null && bank.AddCharacterSource(mainEntry) != null)
                listed.Add(TimeActMergeSourceKey.Of(file.Path));

            Smithbox.Log(typeof(TimeActMergeApplier), LOC.Get("TAE_Merge_Source_Added", file.Path, written.Count));
            report.Added++;
        }

        foreach (var problem in report.Problems)
            Smithbox.LogWarning(typeof(TimeActMergeApplier), TimeActMergeSources.Describe(problem));

        return report;
    }

    /// <summary>Lists a written anibnd as the locator does (as Character Scale adds a new character's files)</summary>
    private static FileDictionaryEntry Register(ProjectEntry project, string path)
    {
        var slash = path.LastIndexOf('/');
        var file = path[(slash + 1)..];
        var dot = file.IndexOf('.');
        var ext = dot < 0 ? "" : file[(dot + 1)..];

        if (ext.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase))
            ext = ext[..^4];

        var entry = new FileDictionaryEntry
        {
            Archive = "",
            Path = path,
            Folder = path[..slash],
            Filename = dot < 0 ? file : file[..dot],
            Extension = ext,
            Validated = true
        };

        var locator = project.Locator;

        foreach (var list in new[] { locator.TimeActFiles, locator.HavokAnimationFiles, locator.FileDictionary })
        {
            if (!list.Entries.Any(e => string.Equals(e.Path, path, StringComparison.OrdinalIgnoreCase)))
                list.Entries.Add(entry);
        }

        return entry;
    }

    #endregion
}

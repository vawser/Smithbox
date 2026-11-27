using Andre.IO.VFS;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

/// <summary>What an Apply or Add File did, for the review panel and the log</summary>
public class GparamMergeApplyReport
{
    /// <summary>Files changed (Apply) or written (Add File)</summary>
    public int Files;

    public int Applied;

    /// <summary>Items left alone because their entry changed since the analysis</summary>
    public int Stale;

    /// <summary>Conflicts still unresolved (Ask Each), kept as mine</summary>
    public int Unresolved;

    public List<string> StaleItems { get; } = new();
    public List<GparamMergeProblem> Problems { get; } = new();
}

/// <summary>
/// Applies a Gparam Merge analysis to this project (Claude/Documentation/Gparam.md §7.3), and adds the
/// files only the source has (D2). UI thread only.
/// </summary>
public static class GparamMergeApplier
{
    /// <summary>
    /// One <see cref="GparamMergeAction"/> per ticked file with chosen items, all in one undo step named after the
    /// source. Unopened files are opened first. Each item is checked against the analysed mine and skipped as stale if
    /// its entry changed since; unresolved conflicts keep mine.
    /// </summary>
    public static GparamMergeApplyReport Apply(GparamEditorView view, ProjectEntry project, GparamMergeAnalysisResult analysis)
    {
        var report = new GparamMergeApplyReport();
        var bank = project.Handler.GparamData.PrimaryBank;
        var actions = new List<GparamMergeAction>();

        foreach (var file in analysis.Files.Where(f => f.Included))
        {
            report.Unresolved += file.Plan.Items.Count(i =>
                i.Included && i.Kind == GparamMergeKind.Conflict && i.Choice == GparamMergeChoice.Unresolved);

            var chosen = GparamMerge.ChosenItems(file.Plan);
            if (chosen.Count == 0)
                continue;

            var entry = (FileDictionaryEntry)file.Target.Tag;

            if (!bank.Entries.TryGetValue(entry, out var wrapper))
            {
                report.Problems.Add(new GparamMergeProblem("File_Gone", entry.Filename));
                continue;
            }

            if (!wrapper.IsLoaded() && !bank.LoadGraphicsParamNow(entry) || wrapper.Gparam == null)
            {
                report.Problems.Add(new GparamMergeProblem("Load_Failed", entry.Path));
                continue;
            }

            actions.Add(new GparamMergeAction(project, entry, wrapper.Gparam, chosen));
        }

        if (actions.Count == 0)
        {
            Log(report, analysis.SourceName);
            return report;
        }

        var compound = new CompoundAction(actions.Cast<EditorAction>().ToList())
        {
            DisplayName = LOC.Get("GPARAM_Merge_Undo_Name", analysis.SourceName)
        };

        view.ActionManager.ExecuteAction(compound);

        foreach (var action in actions)
        {
            var result = action.LastResult;
            if (result == null)
                continue;

            if (result.Applied > 0)
                report.Files++;

            report.Applied += result.Applied;
            report.Stale += result.Stale;
            report.StaleItems.AddRange(result.StaleItems.Select(s => $"{action.FileEntry.Filename}: {s}"));
        }

        Log(report, analysis.SourceName);
        return report;
    }

    private static void Log(GparamMergeApplyReport report, string sourceName)
    {
        Smithbox.Log<GparamMergeAction>(LOC.Get("GPARAM_Merge_Applied", report.Applied, report.Files, sourceName));

        if (report.Stale > 0)
        {
            Smithbox.LogWarning<GparamMergeAction>(LOC.Get("GPARAM_Merge_Applied_Stale", report.Stale,
                string.Join("; ", report.StaleItems.Take(10))));
        }

        if (report.Unresolved > 0)
            Smithbox.LogWarning<GparamMergeAction>(LOC.Get("GPARAM_Merge_Applied_Unresolved", report.Unresolved));

        foreach (var problem in report.Problems)
            Smithbox.LogWarning<GparamMergeAction>(Describe(problem));
    }

    /// <summary>A problem as a localized line: GPARAM_Merge_Problem_{Code} with the subject and detail</summary>
    public static string Describe(GparamMergeProblem problem) =>
        LOC.Get($"GPARAM_Merge_Problem_{problem.Code}", problem.Subject, problem.Detail);

    #region Add File

    /// <summary>
    /// Write files only the source has into this project, as stored (DCX and binder kept), at the path they have
    /// in the source, and list them in the editor (D2). Not undoable: the review asks first (P4). Refuses a file
    /// this project already has (by key or path), one without a known path, and any when the project has no folder.
    /// </summary>
    public static GparamMergeApplyReport AddFiles(ProjectEntry project, IEnumerable<GparamMergeSourceFile> files)
    {
        var report = new GparamMergeApplyReport();
        var bank = project.Handler.GparamData.PrimaryBank;
        var projectFs = project.VFS.ProjectFS;

        if (projectFs == null || projectFs is EmptyVirtualFileSystem)
        {
            report.Problems.Add(new GparamMergeProblem("No_Project_Folder", project.Descriptor.ProjectName));
            LogAdded(report);
            return report;
        }

        foreach (var file in files)
        {
            var path = file.RelativePath;

            if (path == null || file.ReadBytes == null)
            {
                report.Problems.Add(new GparamMergeProblem("Cannot_Add", file.Path));
                continue;
            }

            var taken = bank.Entries.Keys.Any(e => new GparamMergeFileKey(e.Filename, e.Extension).Equals(file.Key));

            if (taken || project.VFS.FS.FileExists(path))
            {
                report.Problems.Add(new GparamMergeProblem("Exists", path));
                continue;
            }

            byte[] bytes;
            try
            {
                bytes = file.ReadBytes();
            }
            catch (Exception e)
            {
                report.Problems.Add(new GparamMergeProblem("Read_Failed", file.Path, e.Message));
                continue;
            }

            if (bytes == null)
            {
                report.Problems.Add(new GparamMergeProblem("Read_Failed", file.Path));
                continue;
            }

            try
            {
                // Creates the folder in the project if it isn't there yet
                projectFs.WriteFile(path, bytes);
            }
            catch (Exception e)
            {
                report.Problems.Add(new GparamMergeProblem("Write_Failed", path, e.Message));
                continue;
            }

            // As the locator lists a project file
            var entry = new FileDictionaryEntry
            {
                Archive = Path.GetFileName(Path.TrimEndingDirectorySeparator(project.Descriptor.ProjectPath)),
                Path = path,
                Folder = Path.GetDirectoryName(path)?.Replace('\\', '/'),
                Filename = file.Key.Filename,
                Extension = file.Key.Extension
            };

            project.Locator.GparamFiles.Entries.Add(entry);
            bank.Entries.Add(entry, new GparamWrapper(entry));

            Smithbox.Log<GparamMergeAction>(LOC.Get("GPARAM_Merge_File_Added", path));
            report.Files++;
        }

        LogAdded(report);
        return report;
    }

    private static void LogAdded(GparamMergeApplyReport report)
    {
        foreach (var problem in report.Problems)
            Smithbox.LogWarning<GparamMergeAction>(Describe(problem));
    }

    #endregion
}

using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

/// <summary>Which items the review lists</summary>
public enum GparamMergeFilter
{
    All,
    Added,
    Changed,
    Conflicts,
    Unresolved,
    Removed
}

/// <summary>
/// The Merge section of the Tools window (Claude/Documentation/Gparam Merge Plan.md §5, P4): pick a source, Analyse
/// (on a worker), review per file and per item with a choice per conflict, then Apply as one undo step. Files only
/// the source has can be added to the project (D2).
/// </summary>
public class GparamMergeTool
{
    private readonly GparamEditorView View;
    private readonly ProjectEntry Project;

    // Source choice
    private ProjectEntry OtherProject;
    private string FolderPath = "";
    private string LooseFilePath = "";
    private string TransferPath = "";

    // Analysis
    private bool IsAnalysing;
    private GparamMergeSource Source;
    private GparamMergeAnalysisResult Analysis;
    private GparamMergeFileResult SelectedFile;
    private GparamMergeFilter Filter = GparamMergeFilter.All;
    private GparamMergeApplyReport LastReport;
    private string LastReportKind;

    // The rows the item table shows, rebuilt when the file or filter changes
    private List<GparamMergeItem> Rows = new();
    private (GparamMergeFileResult, GparamMergeFilter) RowsKey;

    // Files only the source has, ticked for Add File
    private readonly HashSet<GparamMergeSourceFile> AddTicks = new(ReferenceEqualityComparer.Instance);
    private bool OpenAddConfirm;

    private bool ShowProblems;

    // Worker results are applied while drawing, on the UI thread
    private readonly ConcurrentQueue<Action> MainThreadQueue = new();

    public GparamMergeTool(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        FolderPath = "";
        RowsKey = (null, GparamMergeFilter.All);
    }

    /// <summary>Takes finished analyses; called every frame by the view, so a result arrives with the section closed</summary>
    public void ProcessPending()
    {
        while (MainThreadQueue.TryDequeue(out var action))
            action();
    }

    #region Display

    /// <summary>
    /// Merge from File (Data Transfer menus): pick a Data Transfer export or a loose GPARAM, show this section with
    /// it as the source, and analyse it.
    /// </summary>
    public void MergeFromFile()
    {
        if (!PlatformUtils.Instance.OpenFileDialog(LOC.Get("GPARAM_Merge_From_File_Action"), Array.Empty<string>(),
                CFG.Current.GparamEditor.GparamEditor_Merge_Last_Folder, out var path) || string.IsNullOrEmpty(path))
            return;

        CFG.Current.GparamEditor.GparamEditor_Merge_Last_Folder = Path.GetDirectoryName(path) ?? "";

        var isExport = string.Equals(Path.GetExtension(path), ".json", StringComparison.OrdinalIgnoreCase);
        var type = isExport ? GparamMergeSourceType.TransferExport : GparamMergeSourceType.LooseFile;

        CFG.Current.GparamEditor.GparamEditor_Merge_Source_Type = (int)type;

        if (isExport)
            TransferPath = path;
        else
            LooseFilePath = path;

        ClearPlan();

        CFG.Current.Interface.Interface_GparamEditor_ToolWindow = true;
        FocusNext = true;

        Analyse();
    }

    private bool FocusNext;

    public void OnToolWindow()
    {
        if (FocusNext)
        {
            ToolSections.Focus();
            FocusNext = false;
        }

        if (!ToolSections.Header($"{LOC.Get("GPARAM_Merge_Header")}##gparamMergeHeader"))
            return;

        ImGui.BeginChild("GparamMergeSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("GPARAM_Merge_Hint"));

        DisplaySource();
        DisplayOptions();
        DisplayActions();

        if (Analysis != null)
            DisplayPlan();

        DisplayAddConfirm();

        ImGui.EndChild();
    }

    private static GparamMergeSourceType CurrentSourceType =>
        (GparamMergeSourceType)CFG.Current.GparamEditor.GparamEditor_Merge_Source_Type;

    private void DisplaySource()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Merge_Source_Header"), LOC.Get("GPARAM_Merge_Source_Header_TT"));

        SourceRadio(GparamMergeSourceType.Project, "GPARAM_Merge_Source_Project");
        ImGui.SameLine();
        SourceRadio(GparamMergeSourceType.ModFolder, "GPARAM_Merge_Source_Folder");
        ImGui.SameLine();
        SourceRadio(GparamMergeSourceType.LooseFile, "GPARAM_Merge_Source_File");
        ImGui.SameLine();
        SourceRadio(GparamMergeSourceType.TransferExport, "GPARAM_Merge_Source_Transfer");

        switch (CurrentSourceType)
        {
            case GparamMergeSourceType.ModFolder:
                PathPicker(FolderPath, "GPARAM_Merge_Browse_Folder", BrowseFolder);
                break;

            case GparamMergeSourceType.LooseFile:
                PathPicker(LooseFilePath, "GPARAM_Merge_Browse_File", () => BrowseFile(ref LooseFilePath, Array.Empty<string>()));
                break;

            case GparamMergeSourceType.TransferExport:
                PathPicker(TransferPath, "GPARAM_Merge_Browse_Transfer", () => BrowseFile(ref TransferPath, new[] { "json" }));
                break;

            default:
                DisplayProjectList();
                break;
        }
    }

    private void SourceRadio(GparamMergeSourceType type, string locKey)
    {
        if (ImGui.RadioButton($"{LOC.Get(locKey)}##gparamMergeSource{(int)type}", CurrentSourceType == type) &&
            CurrentSourceType != type)
        {
            CFG.Current.GparamEditor.GparamEditor_Merge_Source_Type = (int)type;
            ClearPlan();
        }

        GUI.Tooltip(LOC.Get($"{locKey}_TT"));
    }

    private static void PathPicker(string path, string locKey, Action browse)
    {
        if (ImGui.Button($"{LOC.Get(locKey)}##gparamMergeBrowse"))
            browse();

        GUI.Tooltip(LOC.Get($"{locKey}_TT"));
        ImGui.SameLine();

        // Unformatted: a path can hold '%'
        ImGui.TextUnformatted(string.IsNullOrEmpty(path) ? LOC.Get("GPARAM_Merge_No_Path") : path);
    }

    private void BrowseFolder()
    {
        var start = CFG.Current.GparamEditor.GparamEditor_Merge_Last_Folder;

        if (PlatformUtils.Instance.OpenFolderDialog(LOC.Get("GPARAM_Merge_Browse_Folder"), out var path, start) &&
            !string.IsNullOrEmpty(path) && path != FolderPath)
        {
            FolderPath = path;
            CFG.Current.GparamEditor.GparamEditor_Merge_Last_Folder = path;
            ClearPlan();
        }
    }

    private void BrowseFile(ref string target, IReadOnlyList<string> filters)
    {
        var start = CFG.Current.GparamEditor.GparamEditor_Merge_Last_Folder;

        if (PlatformUtils.Instance.OpenFileDialog(LOC.Get("GPARAM_Merge_Browse_File"), filters, start, out var path) &&
            !string.IsNullOrEmpty(path) && path != target)
        {
            target = path;
            CFG.Current.GparamEditor.GparamEditor_Merge_Last_Folder = Path.GetDirectoryName(path) ?? "";
            ClearPlan();
        }
    }

    private void DisplayProjectList()
    {
        ImGui.BeginChild("gparamMergeProjects", new Vector2(0, 120), ImGuiChildFlags.Borders);

        var shown = 0;

        foreach (var proj in Smithbox.Orchestrator.Projects)
        {
            if (proj == null || proj == Project || proj.Descriptor.ProjectType != Project.Descriptor.ProjectType)
                continue;

            if (ImGui.Selectable($"{proj.Descriptor.ProjectName}##gparamMergeProject{shown}", proj == OtherProject) &&
                proj != OtherProject)
            {
                OtherProject = proj;
                ClearPlan();
            }

            shown++;
        }

        if (shown == 0)
            GUI.WrappedText(LOC.Get("GPARAM_Merge_No_Projects"));

        ImGui.EndChild();
    }

    private void DisplayOptions()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Merge_Options_Header"), LOC.Get("GPARAM_Merge_Options_Header_TT"));

        ImGui.TextUnformatted(LOC.Get("GPARAM_Merge_Conflict_Label"));
        GUI.Tooltip(LOC.Get("GPARAM_Merge_Conflict_Label_TT"));

        ConflictRadio(GparamMergeConflictMode.KeepMine, "GPARAM_Merge_Conflict_Keep_Mine");
        ImGui.SameLine();
        ConflictRadio(GparamMergeConflictMode.TakeTheirs, "GPARAM_Merge_Conflict_Take_Theirs");
        ImGui.SameLine();
        ConflictRadio(GparamMergeConflictMode.AskEach, "GPARAM_Merge_Conflict_Ask_Each");

        var include = CFG.Current.GparamEditor.GparamEditor_Merge_Include_Removals;
        if (ImGui.Checkbox($"{LOC.Get("GPARAM_Merge_Include_Removals")}##gparamMergeRemovals", ref include))
        {
            CFG.Current.GparamEditor.GparamEditor_Merge_Include_Removals = include;
            ClearPlan();
        }

        GUI.Tooltip(LOC.Get("GPARAM_Merge_Include_Removals_TT"));
    }

    private void ConflictRadio(GparamMergeConflictMode mode, string locKey)
    {
        var current = (GparamMergeConflictMode)CFG.Current.GparamEditor.GparamEditor_Merge_Conflict_Mode;

        if (ImGui.RadioButton($"{LOC.Get(locKey)}##gparamMergeConflict{(int)mode}", current == mode) && current != mode)
        {
            CFG.Current.GparamEditor.GparamEditor_Merge_Conflict_Mode = (int)mode;
            ClearPlan();
        }

        GUI.Tooltip(LOC.Get($"{locKey}_TT"));
    }

    private void DisplayActions()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Merge_Actions_Header"), "");

        if (IsAnalysing)
        {
            GUI.WrappedText(LOC.Get("GPARAM_Merge_Analysing"));
            return;
        }

        var unresolved = Analysis?.Files.Where(f => f.Included).Sum(f => f.Plan.Items.Count(IsUnresolved)) ?? 0;
        var canApply = Analysis != null && Analysis.Files.Any(f => f.Included && f.Plan.Items.Any(i => i.TakesTheirs)) &&
                       unresolved == 0;

        GUI.ConditionalMultiButtonInput("gparamMergeActions",
            "gparamMergeAnalyse", LOC.Get("GPARAM_Merge_Analyse"), LOC.Get("GPARAM_Merge_Analyse_TT"),
            Analyse, HasSource,

            "gparamMergeApply", LOC.Get("GPARAM_Merge_Apply"), LOC.Get("GPARAM_Merge_Apply_TT"),
            Apply, canApply,

            "gparamMergeClear", LOC.Get("GPARAM_Merge_Clear"), LOC.Get("GPARAM_Merge_Clear_TT"),
            ClearPlan, Analysis != null);

        if (unresolved > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("GPARAM_Merge_Unresolved_Hint", unresolved));

        if (LastReport != null)
            DisplayReport();
    }

    private void DisplayReport()
    {
        var text = LastReportKind == "Add"
            ? LOC.Get("GPARAM_Merge_Report_Added", LastReport.Files)
            : LOC.Get("GPARAM_Merge_Report_Applied", LastReport.Applied, LastReport.Files);

        GUI.WrappedText(text);

        if (LastReport.Stale > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("GPARAM_Merge_Report_Stale", LastReport.Stale));
            GUI.Tooltip(string.Join("\n", LastReport.StaleItems.Take(30)));
        }

        if (LastReport.Problems.Count > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("GPARAM_Merge_Report_Problems", LastReport.Problems.Count));
            GUI.Tooltip(string.Join("\n", LastReport.Problems.Take(30).Select(GparamMergeApplier.Describe)));
        }
    }

    #endregion

    #region Plan

    private void DisplayPlan()
    {
        var a = Analysis;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Merge_Plan_Header"), LOC.Get("GPARAM_Merge_Plan_Header_TT"));

        GUI.WrappedText(LOC.Get("GPARAM_Merge_Plan_Summary", a.SourceName, a.Files.Count,
            a.Count(GparamMergeKind.Added), a.Count(GparamMergeKind.Changed), a.Count(GparamMergeKind.Conflict),
            a.Count(GparamMergeKind.RemovedByThem), a.Identical));

        DisplayProblems(a);
        DisplayUnmatched(a);

        if (a.Files.Count == 0)
        {
            GUI.WrappedText(LOC.Get("GPARAM_Merge_Plan_Empty"));
            return;
        }

        GUI.MultiButtonInput("gparamMergeBulk",
            "gparamMergeKeepAll", LOC.Get("GPARAM_Merge_Keep_All_Mine"), LOC.Get("GPARAM_Merge_Keep_All_Mine_TT"),
            () => SetAllConflicts(GparamMergeChoice.Mine),

            "gparamMergeTakeAll", LOC.Get("GPARAM_Merge_Take_All_Theirs"), LOC.Get("GPARAM_Merge_Take_All_Theirs_TT"),
            () => SetAllConflicts(GparamMergeChoice.Theirs));

        ImGui.SetNextItemWidth(220);
        if (ImGui.BeginCombo("##gparamMergeFilter", FilterName(Filter)))
        {
            foreach (var filter in Enum.GetValues<GparamMergeFilter>())
            {
                if (ImGui.Selectable(FilterName(filter), filter == Filter))
                    Filter = filter;
            }

            ImGui.EndCombo();
        }

        GUI.Tooltip(LOC.Get("GPARAM_Merge_Filter_TT"));

        DisplayFileList(a);
        DisplayItems();
    }

    private void DisplayProblems(GparamMergeAnalysisResult a)
    {
        var lines = a.Problems.Select(GparamMergeApplier.Describe).Concat(a.Warnings).ToList();
        if (lines.Count == 0)
            return;

        GUI.ConditionalHeader(LOC.Get("GPARAM_Merge_Problems_Header", lines.Count), LOC.Get("GPARAM_Merge_Problems_Header_TT"),
            ref ShowProblems);

        if (!ShowProblems)
            return;

        foreach (var line in lines.Take(50))
            GUI.WrappedText(line);

        if (lines.Count > 50)
            GUI.WrappedText(LOC.Get("GPARAM_Merge_More", lines.Count - 50));
    }

    /// <summary>Files only the source has, each with a tick for Add File</summary>
    private void DisplayUnmatched(GparamMergeAnalysisResult a)
    {
        if (a.Unmatched.Count == 0)
            return;

        GUI.Spacer();
        ImGui.TextUnformatted(LOC.Get("GPARAM_Merge_Unmatched", a.Unmatched.Count));
        GUI.Tooltip(LOC.Get("GPARAM_Merge_Unmatched_TT"));

        var height = Math.Min(a.Unmatched.Count, 6) * ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().WindowPadding.Y * 2;
        ImGui.BeginChild("gparamMergeUnmatched", new Vector2(0, height), ImGuiChildFlags.Borders);

        for (var i = 0; i < a.Unmatched.Count; i++)
        {
            var file = a.Unmatched[i];
            var canAdd = file.RelativePath != null;
            var ticked = AddTicks.Contains(file);

            if (!canAdd)
                ImGui.BeginDisabled();

            if (ImGui.Checkbox($"##gparamMergeAddTick{i}", ref ticked))
            {
                if (ticked)
                    AddTicks.Add(file);
                else
                    AddTicks.Remove(file);
            }

            if (!canAdd)
                ImGui.EndDisabled();

            ImGui.SameLine();
            ImGui.TextUnformatted(file.Path);

            if (!canAdd)
                GUI.Tooltip(LOC.Get("GPARAM_Merge_Cannot_Add_TT"));
        }

        ImGui.EndChild();

        if (GUI.ExplainedButton($"{LOC.Get("GPARAM_Merge_Add_Files", AddTicks.Count)}##gparamMergeAdd",
            LOC.Get("GPARAM_Merge_Add_Files_TT"), AddTicks.Count > 0))
        {
            OpenAddConfirm = true;
        }
    }

    private void DisplayAddConfirm()
    {
        var title = $"{LOC.Get("GPARAM_Merge_Add_Confirm_Title")}##gparamMergeAddConfirm";

        if (OpenAddConfirm)
        {
            ImGui.OpenPopup(title);
            OpenAddConfirm = false;
        }

        if (!ImGui.BeginPopupModal(title, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        ImGui.TextUnformatted(LOC.Get("GPARAM_Merge_Add_Confirm_Text", AddTicks.Count));

        foreach (var file in AddTicks.Take(15))
            ImGui.TextUnformatted($"  {file.RelativePath}");

        if (AddTicks.Count > 15)
            ImGui.TextUnformatted(LOC.Get("GPARAM_Merge_More", AddTicks.Count - 15));

        GUI.Spacer();

        if (ImGui.Button($"{LOC.Get("GPARAM_Merge_Add_Confirm_Yes")}##gparamMergeAddYes"))
        {
            AddFiles();
            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("GPARAM_Merge_Add_Confirm_No")}##gparamMergeAddNo"))
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }

    private void DisplayFileList(GparamMergeAnalysisResult a)
    {
        var height = Math.Min(a.Files.Count, 6) * ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().WindowPadding.Y * 2;
        ImGui.BeginChild("gparamMergeFiles", new Vector2(0, height), ImGuiChildFlags.Borders);

        for (var i = 0; i < a.Files.Count; i++)
        {
            var file = a.Files[i];
            var included = file.Included;

            if (ImGui.Checkbox($"##gparamMergeFileTick{i}", ref included))
                file.Included = included;

            GUI.Tooltip(LOC.Get("GPARAM_Merge_File_Tick_TT"));
            ImGui.SameLine();

            var plan = file.Plan;
            var label = LOC.Get("GPARAM_Merge_File_Row", file.Target.Key.ToString(),
                plan.Count(GparamMergeKind.Added), plan.Count(GparamMergeKind.Changed),
                plan.Count(GparamMergeKind.Conflict), plan.Count(GparamMergeKind.RemovedByThem));

            if (ImGui.Selectable($"{label}##gparamMergeFile{i}", file == SelectedFile))
                SelectedFile = file;
        }

        ImGui.EndChild();
    }

    private void DisplayItems()
    {
        if (SelectedFile == null)
        {
            GUI.WrappedText(LOC.Get("GPARAM_Merge_Select_File"));
            return;
        }

        if (RowsKey != (SelectedFile, Filter))
        {
            Rows = SelectedFile.Plan.Items.Where(Matches).ToList();
            RowsKey = (SelectedFile, Filter);
        }

        if (Rows.Count == 0)
        {
            GUI.WrappedText(LOC.Get("GPARAM_Merge_No_Items"));
            return;
        }

        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable |
                    ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;

        var height = Math.Min(Rows.Count + 1, 16) * ImGui.GetFrameHeightWithSpacing();

        if (!ImGui.BeginTable("gparamMergeItems", 6, flags, new Vector2(0, height)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("GPARAM_Merge_Column_Take"), ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn(LOC.Get("GPARAM_Merge_Column_Entry"), ImGuiTableColumnFlags.WidthStretch, 2.5f);
        ImGui.TableSetupColumn(LOC.Get("GPARAM_Merge_Column_Kind"), ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableSetupColumn(LOC.Get("GPARAM_Merge_Column_Vanilla"), ImGuiTableColumnFlags.WidthStretch, 1.5f);
        ImGui.TableSetupColumn(LOC.Get("GPARAM_Merge_Column_Mine"), ImGuiTableColumnFlags.WidthStretch, 1.5f);
        ImGui.TableSetupColumn(LOC.Get("GPARAM_Merge_Column_Theirs"), ImGuiTableColumnFlags.WidthStretch, 1.5f);
        ImGui.TableHeadersRow();

        var clipper = new ImGuiListClipper();
        clipper.Begin(Rows.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                DisplayRow(Rows[i], i);
        }

        clipper.End();
        ImGui.EndTable();
    }

    private void DisplayRow(GparamMergeItem item, int i)
    {
        ImGui.TableNextRow();

        // Take: a tick, or Mine / Theirs for a conflict
        ImGui.TableNextColumn();

        if (item.Kind == GparamMergeKind.Conflict)
        {
            if (ImGui.RadioButton($"{LOC.Get("GPARAM_Merge_Choice_Mine")}##gparamMergeMine{i}", item.Choice == GparamMergeChoice.Mine))
                item.Choice = GparamMergeChoice.Mine;

            ImGui.SameLine();

            if (ImGui.RadioButton($"{LOC.Get("GPARAM_Merge_Choice_Theirs")}##gparamMergeTheirs{i}", item.Choice == GparamMergeChoice.Theirs))
                item.Choice = GparamMergeChoice.Theirs;
        }
        else
        {
            var included = item.Included;

            if (ImGui.Checkbox($"##gparamMergeItemTick{i}", ref included))
                item.Included = included;

            if (item.Kind == GparamMergeKind.RemovedByThem && item.MineChanged)
                GUI.Tooltip(LOC.Get("GPARAM_Merge_Removed_Mine_Changed_TT"));
        }

        // Entry: click to show it in the editor
        ImGui.TableNextColumn();

        if (ImGui.Selectable($"{EntryLabel(item)}##gparamMergeEntry{i}"))
            Jump(SelectedFile, item);

        GUI.Tooltip(LOC.Get("GPARAM_Merge_Entry_TT", item.Path));

        // Kind
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LOC.Get($"GPARAM_Merge_Kind_{item.Kind}"));

        if (item.Reason != null)
            GUI.Tooltip(LOC.Get($"GPARAM_Merge_Reason_{item.Reason}"));

        // Vanilla / mine / theirs
        foreach (var side in new[] { item.Vanilla, item.Mine, item.Theirs })
        {
            ImGui.TableNextColumn();

            var text = Describe(side, item.Level);
            ImGui.TextUnformatted(Shorten(text));

            if (text.Length > 60)
                GUI.Tooltip(text);
        }
    }

    private bool Matches(GparamMergeItem item) => Filter switch
    {
        GparamMergeFilter.Added => item.Kind == GparamMergeKind.Added,
        GparamMergeFilter.Changed => item.Kind == GparamMergeKind.Changed,
        GparamMergeFilter.Conflicts => item.Kind == GparamMergeKind.Conflict,
        GparamMergeFilter.Unresolved => IsUnresolved(item),
        GparamMergeFilter.Removed => item.Kind == GparamMergeKind.RemovedByThem,
        _ => true
    };

    private static bool IsUnresolved(GparamMergeItem item) =>
        item.Included && item.Kind == GparamMergeKind.Conflict && item.Choice == GparamMergeChoice.Unresolved;

    private static string FilterName(GparamMergeFilter filter) => LOC.Get($"GPARAM_Merge_Filter_{filter}");

    private void SetAllConflicts(GparamMergeChoice choice)
    {
        foreach (var file in Analysis.Files)
        foreach (var item in file.Plan.Items.Where(i => i.Kind == GparamMergeKind.Conflict))
            item.Choice = choice;
    }

    #endregion

    #region Labels

    /// <summary>The entry's path with the annotation names where there are any</summary>
    private string EntryLabel(GparamMergeItem item)
    {
        if (item.Level == GparamMergeLevel.Header)
            return LOC.Get("GPARAM_Merge_Entry_Header");

        var (groupKey, _) = GparamMerge.SplitSlot(item.GroupKey);
        var group = GparamMetaUtils.GetGroupName(Project, groupKey) is { Length: > 0 } gn
            ? $"{gn} ({GparamMerge.DisplaySlot(item.GroupKey)})"
            : GparamMerge.DisplaySlot(item.GroupKey);

        if (item.Level is GparamMergeLevel.Group or GparamMergeLevel.GroupProperties)
        {
            return item.Level == GparamMergeLevel.GroupProperties
                ? LOC.Get("GPARAM_Merge_Entry_Properties", group)
                : group;
        }

        var (fieldKey, _) = GparamMerge.SplitSlot(item.FieldKey);
        var field = GparamMetaUtils.GetFieldName(Project, groupKey, fieldKey) is { Length: > 0 } fn
            ? fn
            : GparamMerge.DisplaySlot(item.FieldKey);

        return item.Level switch
        {
            GparamMergeLevel.Field => $"{group} > {field}",
            GparamMergeLevel.FieldProperties => LOC.Get("GPARAM_Merge_Entry_Properties", $"{group} > {field}"),
            _ => LOC.Get("GPARAM_Merge_Entry_Value", group, field, item.ValueId)
        };
    }

    /// <summary>One side of an item as text: "-" when absent</summary>
    private static string Describe(object side, GparamMergeLevel level)
    {
        switch (side)
        {
            case null:
                return LOC.Get("GPARAM_Merge_Absent");

            case GPARAM g:
                return string.Format(CultureInfo.InvariantCulture, "Unk0D {0}, Unk40 {1}, Unk50 {2}", g.Unk0D, g.Unk40, g.Unk50);

            case GPARAM.Param p when level == GparamMergeLevel.GroupProperties:
                return p.Name ?? "";

            case GPARAM.Param p:
                return LOC.Get("GPARAM_Merge_Group_Fields", p.Fields.Count);

            case GPARAM.IField f when level == GparamMergeLevel.FieldProperties:
                return string.Format(CultureInfo.InvariantCulture, "{0} (Unk {1})", f.Name ?? "", GparamMerge.FieldUnkOf(f));

            case GPARAM.IField f:
                return LOC.Get("GPARAM_Merge_Field_Values", GparamUtils.GetReadableObjectTypeName(f), f.Values.Count);

            case IReadOnlyList<GPARAM.IFieldValue> keyframes:
                // A single value at time 0 shows as the value; keyframes as time: value
                if (keyframes.Count == 1 && keyframes[0].TimeOfDay == 0)
                    return FormatValue(keyframes[0].Value);

                return string.Join("; ", keyframes.Select(k =>
                    string.Format(CultureInfo.InvariantCulture, "{0}: {1}", k.TimeOfDay, FormatValue(k.Value))));
        }

        return side.ToString();
    }

    private static string FormatValue(object value) => GparamFullModMerge.FormatValue(value);

    private static string Shorten(string text) => text.Length > 60 ? text[..57] + "..." : text;

    #endregion

    #region Jump

    /// <summary>Shows the item's group, field and first value in the editor's lists, opening its file</summary>
    private void Jump(GparamMergeFileResult file, GparamMergeItem item)
    {
        var entry = (FileDictionaryEntry)file.Target.Tag;
        var selection = View.Selection;

        if (!selection.IsSelectedFile(entry) || !selection.IsFileSelected())
            selection.SetFileSelection(entry);

        selection.FocusFile = true;

        var gparam = selection._selectedGparam;
        var group = GparamMerge.FindGroup(gparam, item.GroupKey);

        if (item.Level == GparamMergeLevel.Header || group == null)
            return;

        var groupIndex = gparam.Params.IndexOf(group);
        if (selection._selectedParamGroupKey != group.Key || selection._selectedParamGroupIndex != groupIndex)
        {
            selection.SetGparamGroup(groupIndex, group);
            View.FieldListView.InvalidateAddOptions();
        }

        selection.FocusGroup = true;

        var field = GparamMerge.FindField(group, item.FieldKey);
        if (field == null)
            return;

        var fieldIndex = group.Fields.IndexOf(field);
        if (selection._selectedParamFieldKey != field.Key || selection._selectedParamFieldIndex != fieldIndex)
            selection.SetGparamField(fieldIndex, field);

        selection.FocusField = true;

        if (item.Level != GparamMergeLevel.Value)
            return;

        var value = GparamMerge.FindValue(field, item.ValueId);
        if (value == null)
            return;

        selection.SelectSingleFieldValue(field.IndexOfValue(value), value);
        selection.FocusValue = true;
    }

    #endregion

    #region Analyse / Apply

    private bool HasSource => CurrentSourceType switch
    {
        GparamMergeSourceType.ModFolder => Directory.Exists(FolderPath),
        GparamMergeSourceType.LooseFile => File.Exists(LooseFilePath),
        GparamMergeSourceType.TransferExport => File.Exists(TransferPath),
        _ => OtherProject != null
    };

    // Identifies the chosen source, so an analysis finished after another was picked is dropped
    private string CurrentSourceKey => CurrentSourceType switch
    {
        GparamMergeSourceType.ModFolder => $"folder|{FolderPath}",
        GparamMergeSourceType.LooseFile => $"file|{LooseFilePath}",
        GparamMergeSourceType.TransferExport => $"transfer|{TransferPath}",
        _ => $"project|{OtherProject?.Descriptor.ProjectName}"
    } + $"|{CFG.Current.GparamEditor.GparamEditor_Merge_Conflict_Mode}|{CFG.Current.GparamEditor.GparamEditor_Merge_Include_Removals}";

    private void Analyse()
    {
        if (!HasSource || IsAnalysing)
            return;

        RunAnalysis();
    }

    private async void RunAnalysis()
    {
        IsAnalysing = true;
        ClearPlan();

        // Read on the UI thread, before the first await: the options, this project's files (open ones copied
        // now) and, for a transfer export, the selection and the open file it goes over
        var sourceKey = CurrentSourceKey;
        var sourceType = CurrentSourceType;
        var other = OtherProject;
        var folder = FolderPath;
        var loose = LooseFilePath;

        var options = new GparamMergeOptions
        {
            ConflictMode = (GparamMergeConflictMode)CFG.Current.GparamEditor.GparamEditor_Merge_Conflict_Mode,
            IncludeRemovals = CFG.Current.GparamEditor.GparamEditor_Merge_Include_Removals
        };

        List<GparamMergeTarget> targets;
        GparamMergeSource source = null;
        GparamMergeAnalysisResult result = null;
        Exception error = null;

        try
        {
            targets = GparamMergeSources.BuildTargets(Project);

            if (sourceType == GparamMergeSourceType.TransferExport)
                source = GparamMergeSources.FromTransferFile(Project, View.Selection, TransferPath);

            source ??= sourceType switch
            {
                GparamMergeSourceType.Project => await GparamMergeSources.FromProject(Project, other),
                GparamMergeSourceType.ModFolder => await Task.Run(() => GparamMergeSources.FromModFolder(Project, folder)),
                _ => GparamMergeSource.FromLooseFile(loose)
            };

            var run = source;
            result = await Task.Run(() => GparamMergeAnalysis.Run(targets, run, options));
        }
        catch (Exception e)
        {
            error = e;
        }

        MainThreadQueue.Enqueue(() =>
        {
            IsAnalysing = false;

            if (result == null)
            {
                source?.Dispose();
                Smithbox.LogError<GparamMergeTool>(LOC.Get("GPARAM_Merge_Log_Analysis_Failed", source?.Name ?? ""), error);
                return;
            }

            // Another source or option was picked meanwhile
            if (CurrentSourceKey != sourceKey)
            {
                source?.Dispose();
                return;
            }

            Source = source;
            Analysis = result;
            SelectedFile = result.Files.FirstOrDefault();

            Smithbox.Log<GparamMergeTool>(LOC.Get("GPARAM_Merge_Log_Analysed", result.SourceName, result.Files.Count,
                result.ItemCount, result.Identical, result.Unmatched.Count, result.Problems.Count));

            foreach (var problem in result.Problems)
                Smithbox.LogWarning<GparamMergeTool>(GparamMergeApplier.Describe(problem));
        });
    }

    private void Apply()
    {
        if (Analysis == null)
            return;

        LastReport = GparamMergeApplier.Apply(View, Project, Analysis);
        LastReportKind = "Apply";

        // Applied: the plan describes the files as they were; analyse again to see what is left
        ClearPlan(keepReport: true);
    }

    private void AddFiles()
    {
        var files = AddTicks.ToList();
        LastReport = GparamMergeApplier.AddFiles(Project, files);
        LastReportKind = "Add";

        // The added files are this project's now
        var bank = Project.Handler.GparamData.PrimaryBank;
        foreach (var file in files)
        {
            if (bank.Entries.Keys.Any(e => new GparamMergeFileKey(e.Filename, e.Extension).Equals(file.Key)))
            {
                Analysis?.Unmatched.Remove(file);
                AddTicks.Remove(file);
            }
        }
    }

    private void ClearPlan() => ClearPlan(keepReport: false);

    private void ClearPlan(bool keepReport)
    {
        Source?.Dispose();
        Source = null;
        Analysis = null;
        SelectedFile = null;
        Rows = new();
        RowsKey = (null, GparamMergeFilter.All);
        AddTicks.Clear();

        if (!keepReport)
            LastReport = null;
    }

    #endregion
}

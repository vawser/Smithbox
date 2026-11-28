using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>Which items the review lists</summary>
public enum TimeActMergeFilter
{
    All,
    Added,
    Changed,
    Conflicts,
    Unresolved,
    Removed,
    Clips
}

/// <summary>
/// The Merge section of the Tools window (Claude/Documentation/Time Act.md §5.3): pick a source, Analyse
/// (on a worker, <see cref="TimeActMergeAnalyser"/>), review per source and per item with a choice per conflict, then
/// Apply as one undo step in this view's history (D4). Character sources only the other side has can be added to the
/// project. Entry text is never coloured: colour is reserved for vanilla state.
/// </summary>
public class TimeActMergeTool
{
    private readonly TimeActEditorView View;
    private readonly ProjectEntry Project;

    // Source choice
    private ProjectEntry OtherProject;
    private string FolderPath = "";
    private string FilePath = "";

    // Review
    private TimeActMergeSourceResult SelectedSource;
    private TimeActMergeFilter Filter = TimeActMergeFilter.All;
    private TimeActMergeApplyReport LastReport;
    private bool LastReportWasAdd;
    private bool ShowProblems;

    // The rows the item table shows, rebuilt when the source or filter changes
    private List<TimeActMergeItem> Rows = new();
    private (TimeActMergeSourceResult, TimeActMergeFilter) RowsKey;

    // Each row's three sides as text, worked out once
    private readonly Dictionary<TimeActMergeItem, string[]> SideText = new(ReferenceEqualityComparer.Instance);

    // Sources only the other side has, ticked for Add Source
    private readonly HashSet<TimeActMergeSourceFile> AddTicks = new(ReferenceEqualityComparer.Instance);
    private bool OpenAddConfirm;

    private bool FocusNext;

    public TimeActMergeTool(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        View.MergeAnalyser.Finished += OnAnalysed;
    }

    private TimeActMergeAnalyser Analyser => View.MergeAnalyser;
    private TimeActMergeAnalysisResult Analysis => Analyser.Result;

    #region Display

    /// <summary>
    /// Merge from File (File menu): pick an anibnd or a .tae, show this section with it as the source, and analyse it
    /// </summary>
    public void MergeFromFile()
    {
        if (!PlatformUtils.Instance.OpenFileDialog(LOC.Get("TAE_Merge_From_File_Action"), Array.Empty<string>(),
                CFG.Current.TimeActEditor.TimeActEditor_Merge_Last_Folder, out var path) || string.IsNullOrEmpty(path))
            return;

        CFG.Current.TimeActEditor.TimeActEditor_Merge_Last_Folder = Path.GetDirectoryName(path) ?? "";
        CFG.Current.TimeActEditor.TimeActEditor_Merge_Source_Type = (int)TimeActMergeSourceType.LooseFile;
        FilePath = path;

        ClearPlan();

        CFG.Current.Interface.Interface_TimeActEditor_ToolWindow = true;
        FocusNext = true;

        Analyse();
    }

    public void OnToolWindow()
    {
        if (FocusNext)
        {
            ToolSections.Focus();
            FocusNext = false;
        }

        if (!ToolSections.Header($"{LOC.Get("TAE_Merge_Header")}##taeMergeHeader"))
            return;

        ImGui.BeginChild("TaeMergeSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("TAE_Merge_Hint"));

        DisplaySource();
        DisplayOptions();
        DisplayActions();

        if (Analysis != null)
            DisplayPlan();

        DisplayAddConfirm();

        ImGui.EndChild();
    }

    private static TimeActMergeSourceType CurrentSourceType =>
        (TimeActMergeSourceType)CFG.Current.TimeActEditor.TimeActEditor_Merge_Source_Type;

    private void DisplaySource()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_Merge_Source_Header"), LOC.Get("TAE_Merge_Source_Header_TT"));

        SourceRadio(TimeActMergeSourceType.Project, "TAE_Merge_Source_Project");
        ImGui.SameLine();
        SourceRadio(TimeActMergeSourceType.ModFolder, "TAE_Merge_Source_Folder");
        ImGui.SameLine();
        SourceRadio(TimeActMergeSourceType.LooseFile, "TAE_Merge_Source_File");

        switch (CurrentSourceType)
        {
            case TimeActMergeSourceType.ModFolder:
                PathPicker(FolderPath, "TAE_Merge_Browse_Folder", BrowseFolder);
                break;

            case TimeActMergeSourceType.LooseFile:
                PathPicker(FilePath, "TAE_Merge_Browse_File", BrowseFile);
                break;

            default:
                DisplayProjectList();
                break;
        }
    }

    private void SourceRadio(TimeActMergeSourceType type, string locKey)
    {
        if (ImGui.RadioButton($"{LOC.Get(locKey)}##taeMergeSource{(int)type}", CurrentSourceType == type) &&
            CurrentSourceType != type)
        {
            CFG.Current.TimeActEditor.TimeActEditor_Merge_Source_Type = (int)type;
            ClearPlan();
        }

        GUI.Tooltip(LOC.Get($"{locKey}_TT"));
    }

    private static void PathPicker(string path, string locKey, Action browse)
    {
        if (ImGui.Button($"{LOC.Get(locKey)}##taeMergeBrowse"))
            browse();

        GUI.Tooltip(LOC.Get($"{locKey}_TT"));
        ImGui.SameLine();

        // Unformatted: a path can hold '%'
        ImGui.TextUnformatted(string.IsNullOrEmpty(path) ? LOC.Get("TAE_Merge_No_Path") : path);
    }

    private void BrowseFolder()
    {
        var start = CFG.Current.TimeActEditor.TimeActEditor_Merge_Last_Folder;

        if (PlatformUtils.Instance.OpenFolderDialog(LOC.Get("TAE_Merge_Browse_Folder"), out var path, start) &&
            !string.IsNullOrEmpty(path) && path != FolderPath)
        {
            FolderPath = path;
            CFG.Current.TimeActEditor.TimeActEditor_Merge_Last_Folder = path;
            ClearPlan();
        }
    }

    private void BrowseFile()
    {
        var start = CFG.Current.TimeActEditor.TimeActEditor_Merge_Last_Folder;

        if (PlatformUtils.Instance.OpenFileDialog(LOC.Get("TAE_Merge_Browse_File"), Array.Empty<string>(), start, out var path) &&
            !string.IsNullOrEmpty(path) && path != FilePath)
        {
            FilePath = path;
            CFG.Current.TimeActEditor.TimeActEditor_Merge_Last_Folder = Path.GetDirectoryName(path) ?? "";
            ClearPlan();
        }
    }

    private void DisplayProjectList()
    {
        ImGui.BeginChild("taeMergeProjects", new Vector2(0, 120), ImGuiChildFlags.Borders);

        var shown = 0;

        foreach (var proj in Smithbox.Orchestrator.Projects)
        {
            if (proj == null || proj == Project || proj.Descriptor.ProjectType != Project.Descriptor.ProjectType)
                continue;

            if (ImGui.Selectable($"{proj.Descriptor.ProjectName}##taeMergeProject{shown}", proj == OtherProject) &&
                proj != OtherProject)
            {
                OtherProject = proj;
                ClearPlan();
            }

            shown++;
        }

        if (shown == 0)
            GUI.WrappedText(LOC.Get("TAE_Merge_No_Projects"));

        ImGui.EndChild();
    }

    private void DisplayOptions()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_Merge_Options_Header"), LOC.Get("TAE_Merge_Options_Header_TT"));

        ImGui.TextUnformatted(LOC.Get("TAE_Merge_Conflict_Label"));
        GUI.Tooltip(LOC.Get("TAE_Merge_Conflict_Label_TT"));

        ConflictRadio(TimeActMergeConflictMode.KeepMine, "TAE_Merge_Conflict_Keep_Mine");
        ImGui.SameLine();
        ConflictRadio(TimeActMergeConflictMode.TakeTheirs, "TAE_Merge_Conflict_Take_Theirs");
        ImGui.SameLine();
        ConflictRadio(TimeActMergeConflictMode.AskEach, "TAE_Merge_Conflict_Ask_Each");

        var removals = CFG.Current.TimeActEditor.TimeActEditor_Merge_Include_Removals;
        if (ImGui.Checkbox($"{LOC.Get("TAE_Merge_Include_Removals")}##taeMergeRemovals", ref removals))
        {
            CFG.Current.TimeActEditor.TimeActEditor_Merge_Include_Removals = removals;
            ClearPlan();
        }

        GUI.Tooltip(LOC.Get("TAE_Merge_Include_Removals_TT"));

        var clips = CFG.Current.TimeActEditor.TimeActEditor_Merge_Include_Clips;
        if (ImGui.Checkbox($"{LOC.Get("TAE_Merge_Include_Clips")}##taeMergeClips", ref clips))
        {
            CFG.Current.TimeActEditor.TimeActEditor_Merge_Include_Clips = clips;
            ClearPlan();
        }

        GUI.Tooltip(LOC.Get("TAE_Merge_Include_Clips_TT"));
    }

    private void ConflictRadio(TimeActMergeConflictMode mode, string locKey)
    {
        var current = (TimeActMergeConflictMode)CFG.Current.TimeActEditor.TimeActEditor_Merge_Conflict_Mode;

        if (ImGui.RadioButton($"{LOC.Get(locKey)}##taeMergeConflict{(int)mode}", current == mode) && current != mode)
        {
            CFG.Current.TimeActEditor.TimeActEditor_Merge_Conflict_Mode = (int)mode;
            ClearPlan();
        }

        GUI.Tooltip(LOC.Get($"{locKey}_TT"));
    }

    private void DisplayActions()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_Merge_Actions_Header"), "");

        if (Analyser.IsAnalysing)
        {
            GUI.WrappedText(LOC.Get("TAE_Merge_Analysing"));
            return;
        }

        var unresolved = Analysis?.Sources.Where(s => s.Included).Sum(s => s.Plan.Items.Count(IsUnresolved)) ?? 0;
        var canApply = Analysis != null && Analysis.Sources.Any(s => s.Included && s.Plan.Items.Any(i => i.TakesTheirs)) &&
                       unresolved == 0;

        GUI.ConditionalMultiButtonInput("taeMergeActions",
            "taeMergeAnalyse", LOC.Get("TAE_Merge_Analyse"), LOC.Get("TAE_Merge_Analyse_TT"),
            Analyse, HasSource,

            "taeMergeApply", LOC.Get("TAE_Merge_Apply"), LOC.Get("TAE_Merge_Apply_TT"),
            Apply, canApply,

            "taeMergeClear", LOC.Get("TAE_Merge_Clear"), LOC.Get("TAE_Merge_Clear_TT"),
            () => ClearPlan(), Analysis != null);

        if (unresolved > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_Merge_Unresolved_Hint", unresolved));

        // D4: the step lives in this view's history, as an All Sources mass edit does
        if (Analysis != null && Analysis.Sources.Any(s => s.Included && s.Target.Tag != View.Selection.SelectedTimeActContainerWrapper))
            GUI.WrappedText(LOC.Get("TAE_Merge_Undo_Hint"));

        if (Analyser.Error != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_Merge_Analysis_Failed", Analyser.Error));

        if (LastReport != null)
            DisplayReport();
    }

    private void DisplayReport()
    {
        var text = LastReportWasAdd
            ? LOC.Get("TAE_Merge_Report_Added", LastReport.Added)
            : LOC.Get("TAE_Merge_Report_Applied", LastReport.Applied, LastReport.Sources);

        GUI.WrappedText(text);

        if (LastReport.Stale > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_Merge_Report_Stale", LastReport.Stale));
            GUI.Tooltip(string.Join("\n", LastReport.StaleItems.Take(30)));
        }

        if (LastReport.Unresolved > 0)
            GUI.WrappedText(LOC.Get("TAE_Merge_Unresolved", LastReport.Unresolved));

        if (LastReport.Problems.Count > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_Merge_Report_Problems", LastReport.Problems.Count));
            GUI.Tooltip(string.Join("\n", LastReport.Problems.Take(30).Select(TimeActMergeSources.Describe)));
        }
    }

    #endregion

    #region Plan

    private void DisplayPlan()
    {
        var a = Analysis;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_Merge_Plan_Header"), LOC.Get("TAE_Merge_Plan_Header_TT"));

        GUI.WrappedText(LOC.Get("TAE_Merge_Plan_Summary", a.SourceName, a.Sources.Count,
            a.Count(TimeActMergeKind.Added), a.Count(TimeActMergeKind.Changed), a.Count(TimeActMergeKind.Conflict),
            a.Count(TimeActMergeKind.RemovedByThem), a.Identical,
            a.Sources.Sum(s => s.Plan.Items.Count(i => i.Level == TimeActMergeLevel.Clip))));

        DisplayProblems(a);
        DisplayUnmatched(a);

        if (a.Sources.Count == 0)
        {
            GUI.WrappedText(LOC.Get("TAE_Merge_Plan_Empty"));
            return;
        }

        GUI.MultiButtonInput("taeMergeBulk",
            "taeMergeKeepAll", LOC.Get("TAE_Merge_Keep_All_Mine"), LOC.Get("TAE_Merge_Keep_All_Mine_TT"),
            () => SetAllConflicts(TimeActMergeChoice.Mine),

            "taeMergeTakeAll", LOC.Get("TAE_Merge_Take_All_Theirs"), LOC.Get("TAE_Merge_Take_All_Theirs_TT"),
            () => SetAllConflicts(TimeActMergeChoice.Theirs));

        ImGui.SetNextItemWidth(220);
        if (ImGui.BeginCombo("##taeMergeFilter", FilterName(Filter)))
        {
            foreach (var filter in Enum.GetValues<TimeActMergeFilter>())
            {
                if (ImGui.Selectable(FilterName(filter), filter == Filter))
                    Filter = filter;
            }

            ImGui.EndCombo();
        }

        GUI.Tooltip(LOC.Get("TAE_Merge_Filter_TT"));

        DisplaySourceList(a);
        DisplayItems();
    }

    private void DisplayProblems(TimeActMergeAnalysisResult a)
    {
        if (a.Problems.Count == 0)
            return;

        GUI.ConditionalHeader(LOC.Get("TAE_Merge_Problems_Header", a.Problems.Count), LOC.Get("TAE_Merge_Problems_Header_TT"),
            ref ShowProblems);

        if (!ShowProblems)
            return;

        foreach (var problem in a.Problems.Take(50))
            GUI.WrappedText(TimeActMergeSources.Describe(problem));

        if (a.Problems.Count > 50)
            GUI.WrappedText(LOC.Get("TAE_Merge_More", a.Problems.Count - 50));
    }

    /// <summary>Sources only the other side has, each with a tick for Add Source</summary>
    private void DisplayUnmatched(TimeActMergeAnalysisResult a)
    {
        if (a.Unmatched.Count == 0)
            return;

        GUI.Spacer();
        ImGui.TextUnformatted(LOC.Get("TAE_Merge_Unmatched", a.Unmatched.Count));
        GUI.Tooltip(LOC.Get("TAE_Merge_Unmatched_TT"));

        var height = Math.Min(a.Unmatched.Count, 6) * ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().WindowPadding.Y * 2;
        ImGui.BeginChild("taeMergeUnmatched", new Vector2(0, height), ImGuiChildFlags.Borders);

        for (var i = 0; i < a.Unmatched.Count; i++)
        {
            var file = a.Unmatched[i];
            var canAdd = CanAdd(file);
            var ticked = AddTicks.Contains(file);

            if (!canAdd)
                ImGui.BeginDisabled();

            if (ImGui.Checkbox($"##taeMergeAddTick{i}", ref ticked))
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
                GUI.Tooltip(LOC.Get("TAE_Merge_Cannot_Add_TT"));
        }

        ImGui.EndChild();

        if (GUI.ExplainedButton($"{LOC.Get("TAE_Merge_Add_Sources", AddTicks.Count)}##taeMergeAdd",
            LOC.Get("TAE_Merge_Add_Sources_TT"), AddTicks.Count > 0))
        {
            OpenAddConfirm = true;
        }
    }

    private static bool CanAdd(TimeActMergeSourceFile file) =>
        !file.ByFileName && file.OnlyBinderID == null && file.Path.StartsWith("/chr/", StringComparison.OrdinalIgnoreCase);

    private void DisplayAddConfirm()
    {
        var title = $"{LOC.Get("TAE_Merge_Add_Confirm_Title")}##taeMergeAddConfirm";

        if (OpenAddConfirm)
        {
            ImGui.OpenPopup(title);
            OpenAddConfirm = false;
        }

        if (!ImGui.BeginPopupModal(title, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        ImGui.TextUnformatted(LOC.Get("TAE_Merge_Add_Confirm_Text", AddTicks.Count));

        foreach (var file in AddTicks.Take(15))
            ImGui.TextUnformatted($"  {file.Path}");

        if (AddTicks.Count > 15)
            ImGui.TextUnformatted(LOC.Get("TAE_Merge_More", AddTicks.Count - 15));

        GUI.Spacer();

        if (ImGui.Button($"{LOC.Get("TAE_Merge_Add_Confirm_Yes")}##taeMergeAddYes"))
        {
            AddSources();
            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("TAE_Merge_Add_Confirm_No")}##taeMergeAddNo"))
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }

    private void DisplaySourceList(TimeActMergeAnalysisResult a)
    {
        var height = Math.Min(a.Sources.Count, 6) * ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().WindowPadding.Y * 2;
        ImGui.BeginChild("taeMergeSources", new Vector2(0, height), ImGuiChildFlags.Borders);

        for (var i = 0; i < a.Sources.Count; i++)
        {
            var source = a.Sources[i];
            var included = source.Included;

            if (ImGui.Checkbox($"##taeMergeSourceTick{i}", ref included))
                source.Included = included;

            GUI.Tooltip(LOC.Get("TAE_Merge_Source_Tick_TT"));
            ImGui.SameLine();

            var plan = source.Plan;
            var label = LOC.Get("TAE_Merge_Source_Row", source.Target.DisplayName,
                plan.Count(TimeActMergeKind.Added), plan.Count(TimeActMergeKind.Changed),
                plan.Count(TimeActMergeKind.Conflict), plan.Count(TimeActMergeKind.RemovedByThem));

            if (ImGui.Selectable($"{label}##taeMergeSource{i}", source == SelectedSource))
                SelectedSource = source;
        }

        ImGui.EndChild();
    }

    private void DisplayItems()
    {
        if (SelectedSource == null)
        {
            GUI.WrappedText(LOC.Get("TAE_Merge_Select_Source"));
            return;
        }

        if (RowsKey != (SelectedSource, Filter))
        {
            Rows = SelectedSource.Plan.Items.Where(Matches).ToList();
            RowsKey = (SelectedSource, Filter);
        }

        if (Rows.Count == 0)
        {
            GUI.WrappedText(LOC.Get("TAE_Merge_No_Items"));
            return;
        }

        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable |
                    ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;

        var height = Math.Min(Rows.Count + 1, 16) * ImGui.GetFrameHeightWithSpacing();

        if (!ImGui.BeginTable("taeMergeItems", 6, flags, new Vector2(0, height)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("TAE_Merge_Column_Take"), ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn(LOC.Get("TAE_Merge_Column_Entry"), ImGuiTableColumnFlags.WidthStretch, 2.5f);
        ImGui.TableSetupColumn(LOC.Get("TAE_Merge_Column_Kind"), ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableSetupColumn(LOC.Get("TAE_Merge_Column_Vanilla"), ImGuiTableColumnFlags.WidthStretch, 1.5f);
        ImGui.TableSetupColumn(LOC.Get("TAE_Merge_Column_Mine"), ImGuiTableColumnFlags.WidthStretch, 1.5f);
        ImGui.TableSetupColumn(LOC.Get("TAE_Merge_Column_Theirs"), ImGuiTableColumnFlags.WidthStretch, 1.5f);
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

    private void DisplayRow(TimeActMergeItem item, int i)
    {
        ImGui.TableNextRow();

        // Take: a tick, or Mine / Theirs for a conflict
        ImGui.TableNextColumn();

        if (item.Kind == TimeActMergeKind.Conflict)
        {
            if (ImGui.RadioButton($"{LOC.Get("TAE_Merge_Choice_Mine")}##taeMergeMine{i}", item.Choice == TimeActMergeChoice.Mine))
                item.Choice = TimeActMergeChoice.Mine;

            ImGui.SameLine();

            if (ImGui.RadioButton($"{LOC.Get("TAE_Merge_Choice_Theirs")}##taeMergeTheirs{i}", item.Choice == TimeActMergeChoice.Theirs))
                item.Choice = TimeActMergeChoice.Theirs;
        }
        else
        {
            var included = item.Included;

            if (ImGui.Checkbox($"##taeMergeItemTick{i}", ref included))
                item.Included = included;

            if (item.Kind == TimeActMergeKind.RemovedByThem && item.MineChanged)
                GUI.Tooltip(LOC.Get("TAE_Merge_Removed_Mine_Changed_TT"));
        }

        // Entry: click to show it in the editor
        ImGui.TableNextColumn();

        if (ImGui.Selectable($"{EntryLabel(item)}##taeMergeEntry{i}"))
            Jump(SelectedSource, item);

        GUI.Tooltip(LOC.Get("TAE_Merge_Entry_TT", item.Path));

        // Kind
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(LOC.Get($"TAE_Merge_Kind_{item.Kind}"));

        if (item.Reason != null)
            GUI.Tooltip(LOC.Get($"TAE_Merge_Reason_{item.Reason}"));

        // Game / mine / theirs
        if (!SideText.TryGetValue(item, out var sides))
        {
            sides = DescribeSides(item);
            SideText[item] = sides;
        }

        foreach (var text in sides)
        {
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(Shorten(text));

            if (text.Length > 60)
                GUI.Tooltip(text);
        }
    }

    private bool Matches(TimeActMergeItem item) => Filter switch
    {
        TimeActMergeFilter.Added => item.Kind == TimeActMergeKind.Added,
        TimeActMergeFilter.Changed => item.Kind == TimeActMergeKind.Changed,
        TimeActMergeFilter.Conflicts => item.Kind == TimeActMergeKind.Conflict,
        TimeActMergeFilter.Unresolved => IsUnresolved(item),
        TimeActMergeFilter.Removed => item.Kind == TimeActMergeKind.RemovedByThem,
        TimeActMergeFilter.Clips => item.Level == TimeActMergeLevel.Clip,
        _ => true
    };

    private static bool IsUnresolved(TimeActMergeItem item) =>
        item.Included && item.Kind == TimeActMergeKind.Conflict && item.Choice == TimeActMergeChoice.Unresolved;

    private static string FilterName(TimeActMergeFilter filter) => LOC.Get($"TAE_Merge_Filter_{filter}");

    private void SetAllConflicts(TimeActMergeChoice choice)
    {
        foreach (var source in Analysis.Sources)
        foreach (var item in source.Plan.Items.Where(i => i.Kind == TimeActMergeKind.Conflict))
            item.Choice = choice;
    }

    #endregion

    #region Labels

    private TAE.Template.BankTemplate Template(TimeActMergeSourceResult source) =>
        source?.Target.Tag is TimeActContainerWrapper wrapper ? Project.Handler.TimeActData?.GetTemplate(wrapper) : null;

    /// <summary>What the item is: the time act, animation (by its file name) and event, as the editor names them</summary>
    private string EntryLabel(TimeActMergeItem item)
    {
        var wrapper = SelectedSource?.Target.Tag as TimeActContainerWrapper;

        string Animation()
        {
            var entry = wrapper?.TimeActs.FirstOrDefault(e => e.BinderID == item.BinderID);
            var full = entry != null ? entry.GetFullAnimationID(item.AnimationID, wrapper.AnimationCategorySize) : item.AnimationID;
            return wrapper != null ? wrapper.FormatAnimationName(full) : item.AnimationID.ToString();
        }

        return item.Level switch
        {
            TimeActMergeLevel.TimeAct => item.TimeActName,
            TimeActMergeLevel.Header => LOC.Get("TAE_Merge_Entry_Header", item.TimeActName),
            TimeActMergeLevel.Animation => $"{item.TimeActName} > {Animation()}",
            TimeActMergeLevel.AnimationProperties => LOC.Get("TAE_Merge_Entry_Properties", item.TimeActName, Animation()),
            TimeActMergeLevel.AnimationBody => LOC.Get("TAE_Merge_Entry_Events", item.TimeActName, Animation()),
            TimeActMergeLevel.Clip => LOC.Get("TAE_Merge_Entry_Clip",
                wrapper != null ? wrapper.FormatAnimationName(item.AnimationID) : item.AnimationID.ToString()),
            _ => $"{item.TimeActName} > {Animation()} > {EventName((item.Theirs ?? item.Mine ?? item.Vanilla) as TAE.Event)}" +
                 (item.Level == TimeActMergeLevel.EventTiming ? LOC.Get("TAE_Merge_Entry_Timing")
                     : item.Level == TimeActMergeLevel.EventContent ? LOC.Get("TAE_Merge_Entry_Content") : "")
        };
    }

    private string EventName(TAE.Event ev)
    {
        if (ev == null)
            return "";

        var template = Template(SelectedSource);
        var name = template != null && template.TryGetValue(ev.Type, out var eventTemplate) ? eventTemplate.Name : null;

        return $"[{ev.Type}]{(name != null ? $" {name}" : "")} @ {FormatTime(ev.StartTime)}";
    }

    private static string FormatTime(float seconds) =>
        string.Format(CultureInfo.InvariantCulture, "{0:0.###}s ({1:0.#}f)", seconds, seconds * 30);

    private static string FormatEnd(TAE.Event ev) =>
        ev.IsInfiniteEnd ? LOC.Get("TAE_Merge_End_Infinite") : FormatTime(ev.MemeEndTime);

    /// <summary>The game's, mine and theirs as text ("-" when a side lacks it)</summary>
    private string[] DescribeSides(TimeActMergeItem item)
    {
        var sides = new[] { item.Vanilla, item.Mine, item.Theirs };

        if (item.Level == TimeActMergeLevel.EventContent)
            return DescribeContent(sides.Select(e => e as TAE.Event).ToArray());

        // A clip item holds theirs only: the game's and mine follow from what kind of change it is
        if (item.Level == TimeActMergeLevel.Clip)
        {
            var stored = LOC.Get("TAE_Merge_Side_Clip_Stored");
            var absent = LOC.Get("TAE_Merge_Absent");

            var vanilla = item.Kind == TimeActMergeKind.Added || item.Reason == "Both_Added" ? absent : stored;
            var mine = item.Mine is TimeActPendingAnimationFile pending
                ? Describe(pending, item.Level)
                : item.Kind == TimeActMergeKind.Added || item.Reason == "Deleted_Changed" ? absent : stored;

            return [vanilla, mine, Describe(item.Theirs, item.Level)];
        }

        return sides.Select(side => Describe(side, item.Level)).ToArray();
    }

    private static string Describe(object side, TimeActMergeLevel level)
    {
        switch (side)
        {
            case null:
                return LOC.Get("TAE_Merge_Absent");

            case TimeActMergeTimeAct timeAct:
                return timeAct.TimeAct != null
                    ? LOC.Get("TAE_Merge_Side_TimeAct", timeAct.TimeAct.Animations.Count)
                    : timeAct.Name;

            case TAE tae:
                return string.Format(CultureInfo.InvariantCulture, "ID {0}, bank {1}, {2} / {3}", tae.ID, tae.EventBank,
                    tae.SkeletonName ?? "-", tae.SibName ?? "-");

            case TAE.Animation animation when level == TimeActMergeLevel.AnimationProperties:
                return $"{DescribeMiniHeader(animation.MiniHeader)}, {(string.IsNullOrEmpty(animation.AnimFileName) ? "-" : animation.AnimFileName)}";

            case TAE.Animation animation:
                return LOC.Get("TAE_Merge_Side_Events", animation.Events.Count, animation.EventGroups.Count);

            case TAE.Event ev when level == TimeActMergeLevel.EventTiming:
                return $"{FormatTime(ev.StartTime)} - {FormatEnd(ev)}";

            case TAE.Event ev:
                return $"[{ev.Type}] {FormatTime(ev.StartTime)} - {FormatEnd(ev)}";

            case TimeActMergeClip clip:
                return LOC.Get("TAE_Merge_Side_Clip", clip.Bytes.Length);

            case TimeActPendingAnimationFile pending:
                return LOC.Get("TAE_Merge_Side_Clip_Imported", pending.Bytes.Length);
        }

        return side.ToString();
    }

    private static string DescribeMiniHeader(TAE.Animation.AnimMiniHeader header) => header switch
    {
        TAE.Animation.AnimMiniHeader.Standard s => string.Format(CultureInfo.InvariantCulture,
            "loop {0}, HKX {1}", s.IsLoopByDefault ? 1 : 0, s.ImportsHKX ? s.ImportHKXSourceAnimID.ToString() : "-"),
        TAE.Animation.AnimMiniHeader.ImportOtherAnim i => LOC.Get("TAE_Merge_Side_Imports", i.ImportFromAnimID),
        _ => "-"
    };

    /// <summary>
    /// An event's content on each side: the fields that differ between the sides, read through the template when it
    /// fits all of them (else the differing bytes), with Unk04 when that differs
    /// </summary>
    private string[] DescribeContent(TAE.Event[] events)
    {
        var bigEndian = (SelectedSource?.Target.Tag as TimeActContainerWrapper)?.TimeActs.FirstOrDefault()?.TimeAct?.BigEndian ?? false;
        var template = Template(SelectedSource);
        var present = events.Where(e => e != null).ToList();

        var fields = new List<string>[events.Length];
        for (int i = 0; i < events.Length; i++)
            fields[i] = new List<string>();

        if (present.Select(e => e.Unk04).Distinct().Count() > 1)
        {
            for (int i = 0; i < events.Length; i++)
                if (events[i] != null)
                    fields[i].Add($"Unk04 {events[i].Unk04}");
        }

        var values = events.Select(e => e == null ? null : ReadFields(e, template, bigEndian)).ToArray();

        if (values.Where(v => v != null).All(v => v.Count > 0) && present.Count > 0)
        {
            var names = values.First(v => v != null).Keys;

            foreach (var name in names)
            {
                var distinct = values.Where(v => v != null).Select(v => v.GetValueOrDefault(name)).Distinct().Count();
                if (distinct <= 1)
                    continue;

                for (int i = 0; i < events.Length; i++)
                    if (values[i] != null)
                        fields[i].Add($"{name} {values[i].GetValueOrDefault(name)}");
            }
        }
        else
        {
            // No template that fits: the bytes that differ, by offset
            var bytes = events.Select(e => e?.GetParameterBytes(bigEndian)).ToArray();
            var length = bytes.Where(b => b != null).Select(b => b.Length).DefaultIfEmpty(0).Max();

            for (int offset = 0; offset < length; offset++)
            {
                var at = bytes.Where(b => b != null).Select(b => offset < b.Length ? b[offset] : (byte)0).Distinct().Count();
                if (at <= 1)
                    continue;

                for (int i = 0; i < events.Length; i++)
                    if (bytes[i] != null)
                        fields[i].Add($"+{offset:X2} {(offset < bytes[i].Length ? bytes[i][offset] : 0):X2}");
            }
        }

        // The group: one of the game's by position, or the side's own
        for (int i = 0; i < events.Length; i++)
        {
            if (events[i] == null)
                fields[i].Add(LOC.Get("TAE_Merge_Absent"));
            else if (fields[i].Count == 0)
                fields[i].Add(LOC.Get("TAE_Merge_Side_Group_Only"));
        }

        return fields.Select(f => string.Join(", ", f)).ToArray();
    }

    /// <summary>The event's fields through the template, on a copy, when it reads the event back exactly; else none</summary>
    private static Dictionary<string, string> ReadFields(TAE.Event ev, TAE.Template.BankTemplate template, bool bigEndian)
    {
        var result = new Dictionary<string, string>();

        if (template == null || !template.TryGetValue(ev.Type, out var eventTemplate))
            return result;

        try
        {
            var original = ev.GetParameterBytes(bigEndian);
            var copy = new TAE.Event(ev.StartTime, ev.EndTime, ev.Type, ev.Unk04, original.ToArray(), bigEndian);
            copy.ApplyTemplate(bigEndian, eventTemplate, true);

            if (!TimeActCompare.ParametersEqual(original, copy.GetParameterBytes(bigEndian)))
                return new Dictionary<string, string>();

            foreach (var name in eventTemplate.Keys)
            {
                var value = copy.Parameters[name];
                result[name] = value is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) : value?.ToString() ?? "";
            }
        }
        catch (Exception)
        {
            return new Dictionary<string, string>();
        }

        return result;
    }

    private static string Shorten(string text) => text.Length > 60 ? text[..57] + "..." : text;

    #endregion

    #region Jump

    /// <summary>Shows the item's source, time act, animation and event in the editor, opening the source</summary>
    private void Jump(TimeActMergeSourceResult source, TimeActMergeItem item)
    {
        if (source?.Target.Tag is not TimeActContainerWrapper wrapper)
            return;

        var view = View.SourceWindow.SelectSource(wrapper);

        if (item.Level == TimeActMergeLevel.Clip)
            return;

        var entry = wrapper.TimeActs.FirstOrDefault(e => e.BinderID == item.BinderID);
        if (entry == null)
            return;

        view.SelectionWindow.SelectTimeAct(entry);

        if (item.AnimationID < 0 || entry.TimeAct == null)
            return;

        var animation = entry.TimeAct.Animations.FirstOrDefault(e => e.ID == item.AnimationID);
        if (animation == null)
            return;

        view.AnimationSelectionWindow.SelectAnimation(animation);

        // Mine's event, by its index as analysed (the same while the animation is unchanged)
        if (item.MineIndex >= 0 && item.MineIndex < animation.Events.Count)
        {
            view.Selection.SelectedEvents.Clear();
            view.Selection.SelectedEvents.Add(animation.Events[item.MineIndex]);
        }
    }

    #endregion

    #region Analyse / Apply

    private bool HasSource => CurrentSourceType switch
    {
        TimeActMergeSourceType.ModFolder => Directory.Exists(FolderPath),
        TimeActMergeSourceType.LooseFile => File.Exists(FilePath),
        _ => OtherProject != null
    };

    private void Analyse()
    {
        if (!HasSource || Analyser.IsAnalysing)
            return;

        ClearPlan();

        var options = new TimeActMergeOptions
        {
            ConflictMode = (TimeActMergeConflictMode)CFG.Current.TimeActEditor.TimeActEditor_Merge_Conflict_Mode,
            IncludeRemovals = CFG.Current.TimeActEditor.TimeActEditor_Merge_Include_Removals,
            IncludeClips = CFG.Current.TimeActEditor.TimeActEditor_Merge_Include_Clips
        };

        var path = CurrentSourceType == TimeActMergeSourceType.ModFolder ? FolderPath : FilePath;

        Analyser.Start(new TimeActMergeRequest(CurrentSourceType, OtherProject, path, options));
    }

    private void OnAnalysed()
    {
        SelectedSource = Analysis?.Sources.FirstOrDefault();
        RowsKey = (null, TimeActMergeFilter.All);
        SideText.Clear();
        AddTicks.Clear();
        LastReport = null;
    }

    private void Apply()
    {
        if (Analysis == null)
            return;

        LastReport = TimeActMergeApplier.Apply(View, Analysis);
        LastReportWasAdd = false;

        ClearPlan(keepReport: true);
    }

    private void AddSources()
    {
        if (Analyser.Source == null || AddTicks.Count == 0)
            return;

        LastReport = TimeActMergeApplier.AddSources(Project, Analyser.Source, AddTicks.ToList());
        LastReportWasAdd = true;

        // The added sources are this project's now: analysed again they merge like the others
        ClearPlan(keepReport: true);
    }

    private void ClearPlan(bool keepReport = false)
    {
        Analyser.Clear();

        SelectedSource = null;
        Rows = new();
        RowsKey = (null, TimeActMergeFilter.All);
        SideText.Clear();
        AddTicks.Clear();

        if (!keepReport)
            LastReport = null;
    }

    #endregion
}

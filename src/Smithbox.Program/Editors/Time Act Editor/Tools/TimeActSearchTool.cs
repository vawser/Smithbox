using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

public enum TimeActSearchMode
{
    /// <summary>
    /// Searches the selected source
    /// </summary>
    Local,

    /// <summary>
    /// Searches every source of the project
    /// </summary>
    Global
}

/// <summary>
/// Finds animations and events by their names, properties, fields and values. Each result jumps to what it found,
/// selecting the source, time act, animation and event. The matching is done by <see cref="TimeActQueryRunner"/>,
/// in the background, as a global search reads the time acts of every source.
/// </summary>
public class TimeActSearchTool
{
    public TimeActEditorView View;
    public ProjectEntry Project;
    public TimeActSearchMode Mode;

    private enum SearchScope
    {
        CurrentTimeAct,
        AllTimeActs
    }

    // The results stop here, as a value such as 0 is found in nearly every event
    private const int MaxResults = 10000;

    private readonly string Id;
    private readonly TimeActQueryRunner Runner;

    private SearchScope Scope = SearchScope.CurrentTimeAct;
    private string AnimationText = "";
    private string EventText = "";
    private string FieldText = "";
    private string ValueText = "";
    private bool Exact = false;

    private TimeActQueryRun Run;
    // Each result with the label it is listed under, which the filter matches
    private record ResultRow(TimeActQueryMatch Result, string Label);

    private List<ResultRow> Results;
    private TimeActContainerWrapper ResultSource;
    private bool ShowSourceInLabel;
    private bool ShowTimeActInLabel;

    private string ResultFilter = "";
    private bool ExactResultFilter = false;

    // Rebuilt only when the filter changes, and added to as results arrive
    private List<int> FilteredResults;
    private List<ResultRow> FilteredResultsFor;
    private string CachedResultFilter = "";
    private bool CachedExactResultFilter = false;
    private int CheckedResultCount;

    public TimeActSearchTool(TimeActEditorView view, ProjectEntry project, TimeActSearchMode mode)
    {
        View = view;
        Project = project;
        Mode = mode;

        Id = mode is TimeActSearchMode.Local ? "local" : "global";
        Runner = new TimeActQueryRunner(view, project);
    }

    public void OnToolWindow()
    {
        var headerKey = Mode is TimeActSearchMode.Local ? "TAE_SearchTool_Local_Header" : "TAE_SearchTool_Global_Header";

        if (!ToolSections.Header($"{LOC.Get(headerKey)}##timeActSearchToolHeader_{Id}"))
            return;

        ImGui.BeginChild($"TimeActSearchToolSection_{Id}", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get(Mode is TimeActSearchMode.Local ? "TAE_SearchTool_Local_Hint" : "TAE_SearchTool_Global_Hint"));

        var source = View.Selection.SelectedTimeActContainerWrapper;

        if (Mode is TimeActSearchMode.Local)
        {
            // The results point into the source they were found in
            if (Results != null && ResultSource != source)
            {
                ClearResults();
            }

            if (source == null)
            {
                GUI.WrappedText(LOC.Get("TAE_SearchTool_No_Source"));
                return;
            }
        }
        else if (Project.Handler.TimeActData.PrimaryBank is { IsScanning: true } bank)
        {
            GUI.WrappedText(LOC.Get("TAE_SearchTool_Global_Scanning", bank.ScanProgress, bank.ScanTotal));
        }

        if (DisplayInputs())
        {
            StartSearch(source);
        }

        DisplayProgress();
        DisplayResults();
    }

    #region Inputs
    private bool DisplayInputs()
    {
        var search = false;

        // Scope: a global search covers every time act of every source
        if (Mode is TimeActSearchMode.Local)
        {
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TAE_SearchTool_Scope"),
                LOC.Get("TAE_SearchTool_Scope_TT"));

            ImGui.SetNextItemWidth(-1);

            if (ImGui.BeginCombo($"##timeActSearchScope_{Id}", GetScopeName(Scope)))
            {
                foreach (var scope in Enum.GetValues<SearchScope>())
                {
                    if (ImGui.Selectable(GetScopeName(scope), Scope == scope))
                    {
                        Scope = scope;
                    }
                }

                ImGui.EndCombo();
            }
        }

        search |= InputSection("TAE_SearchTool_Animation", $"##timeActSearchAnimation_{Id}", ref AnimationText);
        search |= InputSection("TAE_SearchTool_Event", $"##timeActSearchEvent_{Id}", ref EventText);
        search |= InputSection("TAE_SearchTool_Field", $"##timeActSearchField_{Id}", ref FieldText);
        search |= InputSection("TAE_SearchTool_Value", $"##timeActSearchValue_{Id}", ref ValueText);

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_SearchTool_Options_Header"),
            LOC.Get("TAE_SearchTool_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_SearchTool_Exact")}##timeActSearchExact_{Id}", ref Exact);
        GUI.Tooltip(LOC.Get("TAE_SearchTool_Exact_TT"));

        var query = GetQuery();
        var hasQuery = query.HasAny;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_SearchTool_Actions_Header"),
            LOC.Get("TAE_SearchTool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput($"timeActSearchActions_{Id}",
            $"timeActSearchButton_{Id}",
            LOC.Get("TAE_SearchTool_Search"),
            LOC.Get("TAE_SearchTool_Search_TT"),
            () => search = true,
            hasQuery,

            $"timeActSearchClear_{Id}",
            LOC.Get("TAE_SearchTool_Clear"),
            LOC.Get("TAE_SearchTool_Clear_TT"),
            ClearInputs,
            true);

        return search && hasQuery;
    }

    private void ClearInputs()
    {
        AnimationText = "";
        EventText = "";
        FieldText = "";
        ValueText = "";

        ClearResults();
    }

    /// <summary>
    /// A titled text input. Returns true when the field is left after its text was changed, which Enter does as well,
    /// so a search can be run without the button.
    /// </summary>
    private static bool InputSection(string nameKey, string id, ref string text)
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get(nameKey),
            LOC.Get($"{nameKey}_TT"));

        ImGui.SetNextItemWidth(-1);

        // The text is only kept on the frames the input returns true, so it can't wait for Enter to return:
        // the text would be lost when the field is left by clicking elsewhere
        ImGui.InputText(id, ref text, 255);
        GUI.Tooltip(LOC.Get($"{nameKey}_TT"));

        return ImGui.IsItemDeactivatedAfterEdit();
    }

    private static string GetScopeName(SearchScope scope) => scope switch
    {
        SearchScope.CurrentTimeAct => LOC.Get("TAE_SearchTool_Scope_Current"),
        SearchScope.AllTimeActs => LOC.Get("TAE_SearchTool_Scope_All"),
        _ => $"{scope}"
    };

    private TimeActQuery GetQuery()
    {
        return new TimeActQuery(AnimationText.Trim(), EventText.Trim(), FieldText.Trim(), ValueText.Trim(), Exact);
    }
    #endregion

    #region Search
    private void CancelRun()
    {
        Run?.Cancel();
        Run = null;
    }

    private void ClearResults()
    {
        CancelRun();

        Results = null;
        ResultSource = null;
    }

    /// <summary>
    /// Gathers the sources to search on the main thread, where the editor's lists can be read safely,
    /// then hands them to a background task
    /// </summary>
    private void StartSearch(TimeActContainerWrapper selectedSource)
    {
        ClearResults();

        List<TimeActQuerySource> sources;

        if (Mode is TimeActSearchMode.Local)
        {
            var entries = Scope is SearchScope.AllTimeActs
                ? selectedSource.TimeActs.ToList()
                : View.Selection.SelectedTimeAct != null ? [View.Selection.SelectedTimeAct] : [];

            sources = [Runner.GetLocalSource(selectedSource, entries)];

            ShowSourceInLabel = false;
            ShowTimeActInLabel = Scope is SearchScope.AllTimeActs;
        }
        else
        {
            sources = Runner.GetGlobalSources();

            ShowSourceInLabel = true;
            ShowTimeActInLabel = true;
        }

        Results = new();
        ResultSource = selectedSource;

        // A local search counts its time acts, as there is only the one source
        Run = Runner.Start(GetQuery(), sources, Mode is TimeActSearchMode.Local, MaxResults);
    }
    #endregion

    #region Results
    /// <summary>
    /// Picks up what the search has found so far, and shows how far it has got
    /// </summary>
    private void DisplayProgress()
    {
        var run = Run;

        if (run == null)
            return;

        while (run.Found.TryDequeue(out var result))
        {
            Results?.Add(new ResultRow(result, GetResultLabel(result)));
        }

        if (run.Done)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_SearchTool_Progress_Header"),
            LOC.Get("TAE_SearchTool_Progress_Header_TT"));

        var total = Math.Max(run.Total, 1);
        var fraction = Math.Clamp((float)run.Progress / total, 0, 1);

        ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get("TAE_SearchTool_Progress", run.Progress, run.Total));

        GUI.WrappedText(LOC.Get("TAE_SearchTool_Searching", run.Status));

        GUI.MultiButtonInput($"timeActSearchProgressActions_{Id}",
            $"timeActSearchCancel_{Id}",
            LOC.Get("TAE_SearchTool_Cancel"),
            LOC.Get("TAE_SearchTool_Cancel_TT"),
            CancelRun);
    }

    private string GetResultLabel(TimeActQueryMatch result)
    {
        var label = result.Label;

        if (ShowTimeActInLabel)
            label = $"{result.EntryName}  {label}";

        if (ShowSourceInLabel)
            label = $"{result.Source.Name}  {label}";

        return label;
    }

    private void DisplayResults()
    {
        if (Results == null)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_SearchTool_Results_Header"),
            LOC.Get("TAE_SearchTool_Results_Header_TT"));

        var filtered = GetFilteredResults();

        var truncated = Run is { Truncated: true };

        var countText = truncated
            ? LOC.Get("TAE_SearchTool_Results_Truncated", MaxResults)
            : LOC.Get("TAE_SearchTool_Results", Results.Count);

        if (filtered.Count != Results.Count)
        {
            countText = $"{countText} {LOC.Get("TAE_SearchTool_Results_Filtered", filtered.Count)}";
        }

        GUI.WrappedText(countText);

        if (Results.Count == 0)
            return;

        EditorFilters.DisplayFramedListFilter($"timeActSearchResultFilter_{Id}", ref ResultFilter, ref ExactResultFilter);

        ImGui.BeginChild($"##timeActSearchResults_{Id}", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        TimeActQueryMatch jumpTarget = null;

        var selection = View.Selection;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var (result, label) = Results[index];

                if (ImGui.Button($"{Icons.LocationArrow}##timeActSearchJump_{Id}{index}"))
                {
                    jumpTarget = result;
                }
                GUI.Tooltip(LOC.Get("TAE_SearchTool_Jump_TT"));

                ImGui.SameLine();

                var selected = selection.SelectedTimeActContainerWrapper == result.Source &&
                    selection.SelectedAnimation == result.Animation &&
                    (result.Event == null || selection.IsEventSelected(result.Event));

                if (ImGui.Selectable($"{label}##timeActSearchResult_{Id}{index}", selected))
                {
                    jumpTarget = result;
                }
            }
        }

        clipper.End();

        ImGui.EndChild();

        if (jumpTarget != null)
        {
            Runner.Jump(jumpTarget);
        }
    }

    /// <summary>
    /// The positions of the results that match the filter. The results arrive while the search runs,
    /// so only the new ones are checked, unless the filter has changed.
    /// </summary>
    private List<int> GetFilteredResults()
    {
        if (FilteredResults == null || FilteredResultsFor != Results ||
            CachedResultFilter != ResultFilter || CachedExactResultFilter != ExactResultFilter ||
            CheckedResultCount > Results.Count)
        {
            FilteredResults = new();
            FilteredResultsFor = Results;
            CachedResultFilter = ResultFilter;
            CachedExactResultFilter = ExactResultFilter;
            CheckedResultCount = 0;
        }

        for (int i = CheckedResultCount; i < Results.Count; i++)
        {
            if (EditorFilters.IsMatch(ResultFilter, Results[i].Label, ExactResultFilter))
            {
                FilteredResults.Add(i);
            }
        }

        CheckedResultCount = Results.Count;

        return FilteredResults;
    }
    #endregion
}

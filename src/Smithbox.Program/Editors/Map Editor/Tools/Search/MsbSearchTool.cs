using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public enum MsbSearchMode
{
    /// <summary>
    /// Searches the selected map, or every loaded map
    /// </summary>
    Local,

    /// <summary>
    /// Searches every map of the project, reading the ones that aren't loaded
    /// </summary>
    Global
}

/// <summary>
/// Finds map objects by their names, types, fields and values. Each result jumps to what it found, loading its map
/// first if needed. The matching is done by <see cref="MsbQueryRunner"/>, in the background, as a global search reads
/// every map of the project. Modelled on the Time Act Editor's search.
/// </summary>
public class MsbSearchTool
{
    public MapEditorView View;
    public ProjectEntry Project;
    public MsbSearchMode Mode;

    private enum SearchScope
    {
        SelectedMap,
        LoadedMaps
    }

    // The results stop here, as a value such as 0 is found in nearly every object
    private const int MaxResults = 10000;

    private readonly string Id;
    private readonly MsbQueryRunner Runner;
    private readonly MsbFieldPicker FieldPicker;

    private SearchScope Scope = SearchScope.SelectedMap;
    private string MapText = "";
    private MsbQueryCategory Category = MsbQueryCategory.All;
    private string TypeText = "";
    private string NameText = "";
    private string FieldText = "";
    private string ValueText = "";
    private string Value2Text = "";
    private MsbQueryComparison Comparison = MsbQueryComparison.Contains;
    private bool Exact = false;
    private bool IgnoreDummies = false;

    // What the results were found with, so they can be handed to the mass edit
    private MsbQuery ResultQuery;
    private MsbMassEditScope ResultScope;

    private MsbQueryRun Run;

    private record ResultRow(MsbQueryMatch Result, string Label);

    private List<ResultRow> Results;
    private MapContainer ResultMap;
    private bool ShowMapInLabel;

    private string ResultFilter = "";
    private bool ExactResultFilter = false;

    // Rebuilt only when the filter changes, and added to as results arrive
    private List<int> FilteredResults;
    private List<ResultRow> FilteredResultsFor;
    private string CachedResultFilter = "";
    private bool CachedExactResultFilter = false;
    private int CheckedResultCount;

    // Set by a hand-over from elsewhere: a search runs on the next draw, and may jump to its first result
    private bool PendingSearch;
    private bool PendingJumpToFirst;

    /// <summary>
    /// Set to bring the tool forward in the Tools window
    /// </summary>
    public bool FocusRequested;

    public MsbSearchTool(MapEditorView view, ProjectEntry project, MsbSearchMode mode)
    {
        View = view;
        Project = project;
        Mode = mode;

        Id = mode is MsbSearchMode.Local ? "local" : "global";
        Runner = new MsbQueryRunner(view, project);
        FieldPicker = new MsbFieldPicker(view, project);
    }

    public void OnToolWindow()
    {
        var headerKey = Mode is MsbSearchMode.Local ? "MAP_SearchTool_Local_Header" : "MAP_SearchTool_Global_Header";

        if (FocusRequested)
        {
            FocusRequested = false;
            ToolSections.Focus();
        }

        if (!ToolSections.Header($"{LOC.Get(headerKey)}##msbSearchToolHeader_{Id}"))
            return;

        ImGui.BeginChild($"MsbSearchToolSection_{Id}", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get(Mode is MsbSearchMode.Local ? "MAP_SearchTool_Local_Hint" : "MAP_SearchTool_Global_Hint"));

        if (!Runner.CanUse())
        {
            GUI.WrappedText(LOC.Get("MAP_SearchTool_Unsupported"));
            return;
        }

        var selectedMap = GetSelectedMap();

        if (Mode is MsbSearchMode.Local && Scope is SearchScope.SelectedMap)
        {
            // The results point into the map they were found in
            if (Results != null && ResultMap != selectedMap)
            {
                ClearResults();
            }
        }

        var search = DisplayInputs();

        if (PendingSearch)
        {
            PendingSearch = false;
            search = GetQuery().HasAny && GetScopeUnavailableReason() == null;
        }

        if (search)
        {
            StartSearch(selectedMap);
        }

        DisplayProgress();
        DisplayResults();
    }

    private MapContainer GetSelectedMap()
    {
        var map = View.Selection.SelectedMapContainer;

        return Runner.IsLoadedHere(map) ? map : null;
    }

    #region Targeting
    /// <summary>
    /// Adds a map to the map input, as one of several the search takes ('|' between them). Global only.
    /// </summary>
    public void TargetMap(string mapID)
    {
        if (string.IsNullOrWhiteSpace(MapText))
            MapText = mapID;
        else if (!MapText.Split(EditorFilters.OR_Chr).Any(e => e.Trim() == mapID))
            MapText = $"{MapText}{EditorFilters.OR_Chr}{mapID}";

        Show();
    }

    /// <summary>
    /// Fills the field input
    /// </summary>
    public void TargetField(string field)
    {
        FieldText = field;
        Show();
    }

    /// <summary>
    /// Finds the objects whose field equals the value, and runs the search. With jumpToFirst, the first result is
    /// selected once found.
    /// </summary>
    public void TargetFieldValue(string field, string value, bool jumpToFirst = false)
    {
        FieldText = field;
        ValueText = value ?? "";
        Value2Text = "";
        Comparison = MsbQueryComparison.Equals;

        // The selected map may not hold what is asked for, so every loaded map is searched
        if (Mode is MsbSearchMode.Local && GetSelectedMap() == null)
        {
            Scope = SearchScope.LoadedMaps;
        }

        Show();

        PendingSearch = true;
        PendingJumpToFirst = jumpToFirst;
    }

    private void Show()
    {
        if (Mode is MsbSearchMode.Local)
            CFG.Current.Interface_MapEditor_Tool_LocalSearch = true;
        else
            CFG.Current.Interface_MapEditor_Tool_GlobalSearch = true;

        FocusRequested = true;
    }
    #endregion

    #region Inputs
    private bool DisplayInputs()
    {
        var search = false;

        if (Mode is MsbSearchMode.Local)
        {
            // Scope
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("MAP_SearchTool_Scope"),
                LOC.Get("MAP_SearchTool_Scope_TT"));

            ImGui.SetNextItemWidth(-1);

            if (ImGui.BeginCombo($"##msbSearchScope_{Id}", GetScopeName(Scope)))
            {
                foreach (var scope in Enum.GetValues<SearchScope>())
                {
                    if (ImGui.Selectable($"{GetScopeName(scope)}##msbSearchScope_{Id}_{scope}", Scope == scope))
                    {
                        Scope = scope;
                    }
                    GUI.Tooltip(GetScopeTooltip(scope));
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(GetScopeTooltip(Scope));

            var unavailable = GetScopeUnavailableReason();

            if (unavailable != null)
            {
                GUI.WrappedText(unavailable);
            }
        }
        else
        {
            search |= InputSection("MAP_SearchTool_Map", $"##msbSearchMap_{Id}", ref MapText);
        }

        search |= DisplayCategory();
        search |= InputSection("MAP_SearchTool_Type", $"##msbSearchType_{Id}", ref TypeText);
        search |= InputSection("MAP_SearchTool_Name", $"##msbSearchName_{Id}", ref NameText);

        // Field
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SearchTool_Field"),
            LOC.Get("MAP_SearchTool_Field_TT"));

        FieldPicker.Display($"msbSearchField_{Id}", ref FieldText, "", LOC.Get("MAP_SearchTool_Field_TT"), out var fieldLeft);
        search |= fieldLeft;

        search |= DisplayValueInput();

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SearchTool_Options_Header"),
            LOC.Get("MAP_SearchTool_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_SearchTool_Exact")}##msbSearchExact_{Id}", ref Exact);
        GUI.Tooltip(LOC.Get("MAP_SearchTool_Exact_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_SearchTool_Ignore_Dummies")}##msbSearchIgnoreDummies_{Id}", ref IgnoreDummies);
        GUI.Tooltip(LOC.Get("MAP_SearchTool_Ignore_Dummies_TT"));

        var hasQuery = GetQuery().HasAny && GetScopeUnavailableReason() == null;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SearchTool_Actions_Header"),
            LOC.Get("MAP_SearchTool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput($"msbSearchActions_{Id}",
            $"msbSearchButton_{Id}",
            LOC.Get("MAP_SearchTool_Search"),
            LOC.Get("MAP_SearchTool_Search_TT"),
            () => search = true,
            hasQuery,

            $"msbSearchClear_{Id}",
            LOC.Get("MAP_SearchTool_Clear"),
            LOC.Get("MAP_SearchTool_Clear_TT"),
            ClearInputs,
            true);

        return search && hasQuery;
    }

    /// <summary>
    /// What the scope searches must be there. Gives the reason it isn't, or null.
    /// </summary>
    private string GetScopeUnavailableReason()
    {
        if (Mode is MsbSearchMode.Global)
            return null;

        if (Scope is SearchScope.SelectedMap && GetSelectedMap() == null)
            return LOC.Get("MAP_SearchTool_No_Map");

        if (Scope is SearchScope.LoadedMaps && Runner.GetLoadedMaps().Count == 0)
            return LOC.Get("MAP_SearchTool_No_Loaded_Maps");

        return null;
    }

    private bool DisplayCategory()
    {
        // Category
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SearchTool_Category"),
            LOC.Get("MAP_SearchTool_Category_TT"));

        ImGui.SetNextItemWidth(-1);

        var changed = false;

        if (ImGui.BeginCombo($"##msbSearchCategory_{Id}", GetCategoryName(Category)))
        {
            foreach (var category in Enum.GetValues<MsbQueryCategory>())
            {
                if (ImGui.Selectable($"{GetCategoryName(category)}##msbSearchCategory_{Id}_{category}", Category == category))
                {
                    Category = category;
                    changed = true;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_SearchTool_Category_TT"));

        // Picking a category runs the search again, if it has been run
        return changed && Results != null;
    }

    private void ClearInputs()
    {
        MapText = "";
        Category = MsbQueryCategory.All;
        TypeText = "";
        NameText = "";
        FieldText = "";
        ValueText = "";
        Value2Text = "";

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

    /// <summary>
    /// The value, with how it is compared. Returns true when an input is left after it was changed.
    /// </summary>
    private bool DisplayValueInput()
    {
        var search = false;

        // Value
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SearchTool_Value"),
            LOC.Get("MAP_SearchTool_Value_TT"));

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.35f);

        if (ImGui.BeginCombo($"##msbSearchComparison_{Id}", GetComparisonName(Comparison)))
        {
            foreach (var comparison in Enum.GetValues<MsbQueryComparison>())
            {
                if (ImGui.Selectable($"{GetComparisonName(comparison)}##msbSearchComparison_{Id}_{comparison}",
                    Comparison == comparison))
                {
                    Comparison = comparison;
                }
                GUI.Tooltip(GetComparisonTooltip(comparison));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(GetComparisonTooltip(Comparison));

        ImGui.SameLine();

        if (Comparison is MsbQueryComparison.Between)
        {
            var width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2;

            ImGui.SetNextItemWidth(width);
            ImGui.InputTextWithHint($"##msbSearchValue_{Id}", LOC.Get("MAP_SearchTool_Value_Low_Hint"), ref ValueText, 255);
            GUI.Tooltip(LOC.Get("MAP_SearchTool_Value_Between_TT"));

            search |= ImGui.IsItemDeactivatedAfterEdit();

            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint($"##msbSearchValue2_{Id}", LOC.Get("MAP_SearchTool_Value_High_Hint"), ref Value2Text, 255);
            GUI.Tooltip(LOC.Get("MAP_SearchTool_Value_Between_TT"));

            search |= ImGui.IsItemDeactivatedAfterEdit();
        }
        else
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText($"##msbSearchValue_{Id}", ref ValueText, 255);
            GUI.Tooltip(LOC.Get("MAP_SearchTool_Value_TT"));

            search |= ImGui.IsItemDeactivatedAfterEdit();
        }

        return search;
    }

    private static string GetScopeName(SearchScope scope) => scope switch
    {
        SearchScope.SelectedMap => LOC.Get("MAP_SearchTool_Scope_Selected_Map"),
        SearchScope.LoadedMaps => LOC.Get("MAP_SearchTool_Scope_Loaded_Maps"),
        _ => $"{scope}"
    };

    private static string GetScopeTooltip(SearchScope scope) => scope switch
    {
        SearchScope.SelectedMap => LOC.Get("MAP_SearchTool_Scope_Selected_Map_TT"),
        SearchScope.LoadedMaps => LOC.Get("MAP_SearchTool_Scope_Loaded_Maps_TT"),
        _ => ""
    };

    public static string GetCategoryName(MsbQueryCategory category) => category switch
    {
        MsbQueryCategory.All => LOC.Get("MAP_SearchTool_Category_All"),
        MsbQueryCategory.Parts => LOC.Get("MAP_SearchTool_Category_Parts"),
        MsbQueryCategory.Regions => LOC.Get("MAP_SearchTool_Category_Regions"),
        MsbQueryCategory.Events => LOC.Get("MAP_SearchTool_Category_Events"),
        _ => $"{category}"
    };

    public static string GetComparisonName(MsbQueryComparison comparison) => comparison switch
    {
        MsbQueryComparison.Contains => LOC.Get("MAP_SearchTool_Comparison_Contains"),
        MsbQueryComparison.Equals => "=",
        MsbQueryComparison.NotEquals => "!=",
        MsbQueryComparison.Less => "<",
        MsbQueryComparison.LessOrEqual => "<=",
        MsbQueryComparison.Greater => ">",
        MsbQueryComparison.GreaterOrEqual => ">=",
        MsbQueryComparison.Between => LOC.Get("MAP_SearchTool_Comparison_Between"),
        _ => $"{comparison}"
    };

    public static string GetComparisonTooltip(MsbQueryComparison comparison) => comparison switch
    {
        MsbQueryComparison.Contains => LOC.Get("MAP_SearchTool_Comparison_Contains_TT"),
        MsbQueryComparison.Equals => LOC.Get("MAP_SearchTool_Comparison_Equals_TT"),
        MsbQueryComparison.NotEquals => LOC.Get("MAP_SearchTool_Comparison_Not_Equals_TT"),
        MsbQueryComparison.Less => LOC.Get("MAP_SearchTool_Comparison_Less_TT"),
        MsbQueryComparison.LessOrEqual => LOC.Get("MAP_SearchTool_Comparison_Less_Or_Equal_TT"),
        MsbQueryComparison.Greater => LOC.Get("MAP_SearchTool_Comparison_Greater_TT"),
        MsbQueryComparison.GreaterOrEqual => LOC.Get("MAP_SearchTool_Comparison_Greater_Or_Equal_TT"),
        MsbQueryComparison.Between => LOC.Get("MAP_SearchTool_Comparison_Between_TT"),
        _ => ""
    };

    private MsbQuery GetQuery()
    {
        var map = Mode is MsbSearchMode.Global ? MapText.Trim() : "";

        return new MsbQuery(map, Category, TypeText.Trim(), NameText.Trim(), FieldText.Trim(), ValueText.Trim(), Exact,
            Comparison, Value2Text.Trim(), IgnoreDummies);
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
        ResultMap = null;
    }

    /// <summary>
    /// Gathers the maps to search on the main thread, where the editor's lists can be read safely,
    /// then hands them to a background task
    /// </summary>
    private void StartSearch(MapContainer selectedMap)
    {
        ClearResults();

        List<MsbQuerySource> sources;

        if (Mode is MsbSearchMode.Global)
        {
            sources = Runner.GetGlobalSources();
            ShowMapInLabel = true;
            ResultScope = MsbMassEditScope.AllMaps;
        }
        else if (Scope is SearchScope.LoadedMaps)
        {
            sources = Runner.GetLoadedSources();
            ShowMapInLabel = true;
            ResultScope = MsbMassEditScope.LoadedMaps;
        }
        else
        {
            sources = [Runner.GetLoadedSource(selectedMap)];
            ShowMapInLabel = false;
            ResultScope = MsbMassEditScope.SelectedMap;
        }

        Results = new();
        ResultMap = selectedMap;
        ResultQuery = GetQuery();

        Run = Runner.Start(ResultQuery, sources, MaxResults);
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

        // A hand-over asked for the first result to be selected
        if (PendingJumpToFirst && Results is { Count: > 0 })
        {
            PendingJumpToFirst = false;
            Runner.Jump(Results[0].Result);
        }

        if (run.Done)
        {
            PendingJumpToFirst = false;
            return;
        }

        // Progress
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SearchTool_Progress_Header"),
            LOC.Get("MAP_SearchTool_Progress_Header_TT"));

        var total = Math.Max(run.Total, 1);
        var fraction = Math.Clamp((float)run.Progress / total, 0, 1);

        ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get("MAP_SearchTool_Progress", run.Progress, run.Total));

        GUI.WrappedText(LOC.Get("MAP_SearchTool_Searching", run.Status));

        GUI.MultiButtonInput($"msbSearchProgressActions_{Id}",
            $"msbSearchCancel_{Id}",
            LOC.Get("MAP_SearchTool_Cancel"),
            LOC.Get("MAP_SearchTool_Cancel_TT"),
            CancelRun);
    }

    private string GetResultLabel(MsbQueryMatch result)
    {
        return ShowMapInLabel ? $"{result.MapID}  {result.Label}" : result.Label;
    }

    private void DisplayResults()
    {
        if (Results == null)
            return;

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SearchTool_Results_Header"),
            LOC.Get("MAP_SearchTool_Results_Header_TT"));

        var filtered = GetFilteredResults();

        var countText = Run is { Truncated: true }
            ? LOC.Get("MAP_SearchTool_Results_Truncated", MaxResults)
            : LOC.Get("MAP_SearchTool_Results", Results.Count);

        if (filtered.Count != Results.Count)
        {
            countText = $"{countText} {LOC.Get("MAP_SearchTool_Results_Filtered", filtered.Count)}";
        }

        GUI.WrappedText(countText);

        if (Results.Count == 0)
            return;

        // The results can be edited all at once, by finding them again in the mass edit
        GUI.MultiButtonInput($"msbSearchResultActions_{Id}",
            $"msbSearchSend_{Id}",
            LOC.Get("MAP_SearchTool_Send_To_Mass_Edit"),
            LOC.Get("MAP_SearchTool_Send_To_Mass_Edit_TT"),
            () => View.MassEditTool.LoadQuery(ResultQuery, ResultScope),

            $"msbSearchCopy_{Id}",
            LOC.Get("MAP_SearchTool_Copy_Results"),
            LOC.Get("MAP_SearchTool_Copy_Results_TT"),
            () => CopyResults(filtered));

        EditorFilters.DisplayFramedListFilter($"msbSearchResultFilter_{Id}", ref ResultFilter, ref ExactResultFilter);

        ImGui.BeginChild($"##msbSearchResults_{Id}", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        MsbQueryMatch jumpTarget = null;

        var selection = View.ViewportSelection.GetSelection();

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var (result, label) = Results[index];

                if (ImGui.Button($"{Icons.LocationArrow}##msbSearchJump_{Id}{index}"))
                {
                    jumpTarget = result;
                }
                GUI.Tooltip(LOC.Get("MAP_SearchTool_Jump_TT"));

                ImGui.SameLine();

                var selected = result.Entry.Entity != null && selection.Contains(result.Entry.Entity);

                if (ImGui.Selectable($"{label}##msbSearchResult_{Id}{index}", selected))
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
    /// Copies the results on show, one per line
    /// </summary>
    private void CopyResults(List<int> filtered)
    {
        var lines = filtered.Select(i => Results[i].Label);

        PlatformUtils.Instance.SetClipboardText(string.Join(Environment.NewLine, lines));
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

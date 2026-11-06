using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Finds map objects, then edits or deletes all of them at once. The Find tab narrows down the matches; the Edit tab
/// works out what each of them will become, shows it, and applies it. Modelled on the Time Act Editor's mass edit,
/// and runs its finds through the search's <see cref="MsbQueryRunner"/>.
/// </summary>
public partial class MsbMassEditTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    private readonly MsbQueryRunner Runner;

    /// <summary>
    /// A condition as it is being edited. It is copied into a <see cref="MsbMassEditCondition"/> on Find.
    /// </summary>
    private class ConditionInput
    {
        public string Field = "";
        public MsbQueryComparison Comparison = MsbQueryComparison.Equals;
        public string Value = "";
        public string Value2 = "";
        public MsbFieldPicker Picker;

        public MsbMassEditCondition ToCondition() => new(Field.Trim(), Comparison, Value.Trim(), Value2.Trim());
    }

    private MsbMassEditScope Scope = MsbMassEditScope.SelectedMap;

    private string MapText = "";
    private MsbQueryCategory Category = MsbQueryCategory.All;
    private string TypeText = "";
    private string NameText = "";
    private List<ConditionInput> Conditions = new();
    private bool Exact = false;
    private bool IgnoreDummies = false;

    // Raised by every change to the inputs, so the matches can tell they no longer show what the inputs ask for
    private int InputsVersion;
    private int FoundVersion = -1;

    // What the scope pointed to when Find was pressed
    private MapContainer FoundMap;
    private object[] FoundObjects;

    private MsbQueryRun Run;

    private record ResultRow(MsbQueryMatch Match, string Label);

    private List<ResultRow> Results;
    private HashSet<string> ResultMaps;
    private bool ResultsAcrossMaps;
    private MapContainer ResultMap;
    private MsbMassEditScope ResultScope;

    private string ResultFilter = "";
    private bool ExactResultFilter = false;

    // Toggled by the eye on the list's header
    private bool ShowMatchesList = true;

    // Rebuilt only when the filter changes, and added to as results arrive
    private List<int> FilteredResults;
    private List<ResultRow> FilteredResultsFor;
    private string CachedResultFilter = "";
    private bool CachedExactResultFilter = false;
    private int CheckedResultCount;

    // Set by a search handing its query over, so Find runs on the next draw and the Find tab is brought forward
    private bool PendingFind;
    private bool SelectFindTab;

    /// <summary>
    /// Set to bring the tool forward in the Tools window
    /// </summary>
    public bool FocusRequested;

    public MsbMassEditTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Runner = new MsbQueryRunner(view, project);
        Planner = new MsbMassEditPlanner(project);
    }

    public void OnToolWindow()
    {
        if (FocusRequested)
        {
            FocusRequested = false;
            ToolSections.Focus();
        }

        if (!ToolSections.Header($"{LOC.Get("MAP_MassEditTool_Header")}##msbMassEditToolHeader"))
            return;

        ImGui.BeginChild("MsbMassEditToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("MAP_MassEditTool_Hint"));

        if (!Runner.CanUse())
        {
            GUI.WrappedText(LOC.Get("MAP_SearchTool_Unsupported"));
            return;
        }

        // Matches in one map point into it. Matches across maps are kept while the user jumps between them, which is
        // how they are looked at.
        if (Results != null && !ResultsAcrossMaps && ResultMap != GetSelectedMap())
        {
            ClearResults();
        }

        // A search handed its query over
        if (PendingFind)
        {
            PendingFind = false;

            if (GetScopeUnavailableReason() == null)
            {
                StartFind();
            }
        }

        // Picked up whichever tab is open, so a find keeps going while the Edit tab is shown
        PickUpResults();

        if (ImGui.BeginTabBar("##msbMassEditTabs"))
        {
            var findFlags = SelectFindTab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;
            SelectFindTab = false;

            // Find
            if (ImGui.BeginTabItem($"{LOC.Get("MAP_MassEditTool_Tab_Find")}##msbMassEditTabFind", findFlags))
            {
                DisplayScope();
                DisplayFilter();

                if (DisplayActions())
                {
                    StartFind();
                }

                DisplayProgress();
                DisplayResults();

                ImGui.EndTabItem();
            }
            GUI.Tooltip(LOC.Get("MAP_MassEditTool_Tab_Find_TT"));

            // Edit
            if (ImGui.BeginTabItem($"{LOC.Get("MAP_MassEditTool_Tab_Edit")}##msbMassEditTabEdit"))
            {
                DisplayEdit();

                ImGui.EndTabItem();
            }
            GUI.Tooltip(LOC.Get("MAP_MassEditTool_Tab_Edit_TT"));

            ImGui.EndTabBar();
        }

        DisplayPopups();
    }

    private void Changed()
    {
        InputsVersion++;
    }

    private MapContainer GetSelectedMap()
    {
        var map = View.Selection.SelectedMapContainer;

        return Runner.IsLoadedHere(map) ? map : null;
    }

    /// <summary>
    /// The selected parts, regions and events
    /// </summary>
    private List<MsbEntity> GetSelectedObjects()
    {
        return View.ViewportSelection.GetSelection().OfType<MsbEntity>()
            .Where(e => e.Type is MsbEntityType.Part or MsbEntityType.Region or MsbEntityType.Event && e.WrappedObject is IMsbEntry)
            .ToList();
    }

    /// <summary>
    /// Takes over the inputs of a search, so its results can be edited all at once. Its field and value become a
    /// condition, with an exact search's contains turned into equals. The tool is shown, and Find runs once it is
    /// next drawn.
    /// </summary>
    public void LoadQuery(MsbQuery query, MsbMassEditScope scope)
    {
        Scope = scope;

        MapText = query.Map ?? "";
        Category = query.Category;
        TypeText = query.Type ?? "";
        NameText = query.Name ?? "";
        Exact = query.Exact;
        IgnoreDummies = query.IgnoreDummies;

        Conditions = new();

        if (query.HasField || query.HasValue)
        {
            var condition = NewCondition();

            condition.Field = query.Field ?? "";
            condition.Comparison = query.Comparison is MsbQueryComparison.Contains
                ? query.Exact ? MsbQueryComparison.Equals : MsbQueryComparison.Contains
                : query.Comparison;
            condition.Value = query.Value ?? "";
            condition.Value2 = query.Value2 ?? "";

            Conditions.Add(condition);
        }

        Changed();
        ClearResults();

        CFG.Current.Interface_MapEditor_Tool_MassEdit = true;
        FocusRequested = true;

        PendingFind = true;
        SelectFindTab = true;
    }

    private ConditionInput NewCondition() => new() { Picker = new MsbFieldPicker(View, Project) };

    #region Scope
    private void DisplayScope()
    {
        // Scope
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_MassEditTool_Scope_Header"),
            LOC.Get("MAP_MassEditTool_Scope_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##msbMassEditScope", GetScopeName(Scope)))
        {
            foreach (var scope in Enum.GetValues<MsbMassEditScope>())
            {
                if (ImGui.Selectable($"{GetScopeName(scope)}##msbMassEditScope{scope}", Scope == scope))
                {
                    Scope = scope;
                    Changed();
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

    /// <summary>
    /// The scope needs the selection to point at what it covers. Gives the reason it can't be used, or null.
    /// </summary>
    private string GetScopeUnavailableReason() => Scope switch
    {
        MsbMassEditScope.SelectedObjects when GetSelectedObjects().Count == 0 => LOC.Get("MAP_MassEditTool_Scope_No_Objects"),
        MsbMassEditScope.SelectedMap when GetSelectedMap() == null => LOC.Get("MAP_SearchTool_No_Map"),
        MsbMassEditScope.LoadedMaps when Runner.GetLoadedMaps().Count == 0 => LOC.Get("MAP_SearchTool_No_Loaded_Maps"),
        _ => null
    };

    private static string GetScopeName(MsbMassEditScope scope) => scope switch
    {
        MsbMassEditScope.SelectedObjects => LOC.Get("MAP_MassEditTool_Scope_Selected_Objects"),
        MsbMassEditScope.SelectedMap => LOC.Get("MAP_SearchTool_Scope_Selected_Map"),
        MsbMassEditScope.LoadedMaps => LOC.Get("MAP_SearchTool_Scope_Loaded_Maps"),
        MsbMassEditScope.AllMaps => LOC.Get("MAP_MassEditTool_Scope_All_Maps"),
        _ => $"{scope}"
    };

    private static string GetScopeTooltip(MsbMassEditScope scope) => scope switch
    {
        MsbMassEditScope.SelectedObjects => LOC.Get("MAP_MassEditTool_Scope_Selected_Objects_TT"),
        MsbMassEditScope.SelectedMap => LOC.Get("MAP_SearchTool_Scope_Selected_Map_TT"),
        MsbMassEditScope.LoadedMaps => LOC.Get("MAP_SearchTool_Scope_Loaded_Maps_TT"),
        MsbMassEditScope.AllMaps => LOC.Get("MAP_MassEditTool_Scope_All_Maps_TT"),
        _ => ""
    };
    #endregion

    #region Filter
    private void DisplayFilter()
    {
        // Only a find across every map is narrowed down by map
        if (Scope is MsbMassEditScope.AllMaps)
        {
            TextInput("MAP_SearchTool_Map", "##msbMassEditMap", ref MapText);
        }

        // Category
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SearchTool_Category"),
            LOC.Get("MAP_SearchTool_Category_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##msbMassEditCategory", MsbSearchTool.GetCategoryName(Category)))
        {
            foreach (var category in Enum.GetValues<MsbQueryCategory>())
            {
                if (ImGui.Selectable($"{MsbSearchTool.GetCategoryName(category)}##msbMassEditCategory{category}", Category == category))
                {
                    Category = category;
                    Changed();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_SearchTool_Category_TT"));

        TextInput("MAP_SearchTool_Type", "##msbMassEditType", ref TypeText);
        TextInput("MAP_SearchTool_Name", "##msbMassEditName", ref NameText);

        DisplayConditions();

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SearchTool_Options_Header"),
            LOC.Get("MAP_SearchTool_Options_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_SearchTool_Exact")}##msbMassEditExact", ref Exact))
        {
            Changed();
        }
        GUI.Tooltip(LOC.Get("MAP_SearchTool_Exact_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_SearchTool_Ignore_Dummies")}##msbMassEditIgnoreDummies", ref IgnoreDummies))
        {
            Changed();
        }
        GUI.Tooltip(LOC.Get("MAP_SearchTool_Ignore_Dummies_TT"));
    }

    private void TextInput(string nameKey, string id, ref string text)
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get(nameKey),
            LOC.Get($"{nameKey}_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputText(id, ref text, 255))
        {
            Changed();
        }
        GUI.Tooltip(LOC.Get($"{nameKey}_TT"));
    }

    /// <summary>
    /// The rows of conditions on the fields: [field] then [comparison] [value] [-], all of which must match
    /// </summary>
    private void DisplayConditions()
    {
        // Conditions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_MassEditTool_Conditions_Header"),
            LOC.Get("MAP_MassEditTool_Conditions_Header_TT"));

        ConditionInput removed = null;

        var spacing = ImGui.GetStyle().ItemSpacing.X;

        for (int i = 0; i < Conditions.Count; i++)
        {
            var condition = Conditions[i];

            if (i > 0)
            {
                ImGui.Separator();
            }

            // Field
            if (condition.Picker.Display($"msbMassEditConditionField{i}", ref condition.Field,
                LOC.Get("MAP_MassEditTool_Condition_Field_Hint"), LOC.Get("MAP_SearchTool_Field_TT"), out _))
            {
                Changed();
            }

            // Comparison
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.3f);

            if (ImGui.BeginCombo($"##msbMassEditConditionComparison{i}", MsbSearchTool.GetComparisonName(condition.Comparison)))
            {
                foreach (var comparison in Enum.GetValues<MsbQueryComparison>())
                {
                    if (ImGui.Selectable($"{MsbSearchTool.GetComparisonName(comparison)}##msbMassEditConditionComparison{i}_{comparison}",
                        condition.Comparison == comparison))
                    {
                        condition.Comparison = comparison;
                        Changed();
                    }
                    GUI.Tooltip(MsbSearchTool.GetComparisonTooltip(comparison));
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(MsbSearchTool.GetComparisonTooltip(condition.Comparison));

            ImGui.SameLine();

            var removeWidth = DPI.IconButtonSize.X + spacing;

            // Value
            if (condition.Comparison is MsbQueryComparison.Between)
            {
                var width = (ImGui.GetContentRegionAvail().X - removeWidth - spacing) / 2;

                ImGui.SetNextItemWidth(width);

                if (ImGui.InputTextWithHint($"##msbMassEditConditionValue{i}", LOC.Get("MAP_SearchTool_Value_Low_Hint"),
                    ref condition.Value, 255))
                {
                    Changed();
                }
                GUI.Tooltip(LOC.Get("MAP_SearchTool_Value_Between_TT"));

                ImGui.SameLine();
                ImGui.SetNextItemWidth(width);

                if (ImGui.InputTextWithHint($"##msbMassEditConditionValue2{i}", LOC.Get("MAP_SearchTool_Value_High_Hint"),
                    ref condition.Value2, 255))
                {
                    Changed();
                }
                GUI.Tooltip(LOC.Get("MAP_SearchTool_Value_Between_TT"));
            }
            else
            {
                ImGui.SetNextItemWidth(-removeWidth);

                if (ImGui.InputTextWithHint($"##msbMassEditConditionValue{i}", LOC.Get("MAP_MassEditTool_Condition_Value_Hint"),
                    ref condition.Value, 255))
                {
                    Changed();
                }
                GUI.Tooltip(LOC.Get("MAP_SearchTool_Value_TT"));
            }

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Minus}##msbMassEditConditionRemove{i}", DPI.IconButtonSize))
            {
                removed = condition;
            }
            GUI.Tooltip(LOC.Get("MAP_MassEditTool_Condition_Remove_TT"));
        }

        if (removed != null)
        {
            Conditions.Remove(removed);
            Changed();
        }

        GUI.MultiButtonInput("msbMassEditConditionActions",
            "msbMassEditConditionAdd",
            LOC.Get("MAP_MassEditTool_Condition_Add"),
            LOC.Get("MAP_MassEditTool_Condition_Add_TT"),
            () =>
            {
                Conditions.Add(NewCondition());
                Changed();
            });
    }
    #endregion

    #region Find
    private bool DisplayActions()
    {
        var find = false;
        var canFind = GetScopeUnavailableReason() == null;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_MassEditTool_Actions_Header"),
            LOC.Get("MAP_MassEditTool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("msbMassEditActions",
            "msbMassEditFind",
            LOC.Get("MAP_MassEditTool_Find"),
            LOC.Get("MAP_MassEditTool_Find_TT"),
            () => find = true,
            canFind,

            "msbMassEditClear",
            LOC.Get("MAP_MassEditTool_Clear"),
            LOC.Get("MAP_MassEditTool_Clear_TT"),
            ClearInputs,
            true);

        return find && canFind;
    }

    private void ClearInputs()
    {
        MapText = "";
        Category = MsbQueryCategory.All;
        TypeText = "";
        NameText = "";
        Conditions = new();

        Changed();
        ClearResults();
    }

    private void CancelRun()
    {
        Run?.Cancel();
        Run = null;
    }

    private void ClearResults()
    {
        CancelRun();

        Results = null;
        ResultMaps = null;
        ResultMap = null;
        ResultsAcrossMaps = false;
        FoundVersion = -1;

        ClearPlan();
    }

    /// <summary>
    /// Takes what the scope covers from the editor on the main thread, then hands the filter to a background task
    /// </summary>
    private void StartFind()
    {
        ClearResults();

        var selectedMap = GetSelectedMap();
        var selectedObjects = GetSelectedObjects();

        List<MsbQuerySource> sources;
        HashSet<object> limit = null;

        switch (Scope)
        {
            case MsbMassEditScope.SelectedObjects:
                sources = selectedObjects.Select(e => e.Container).OfType<MapContainer>().Distinct()
                    .Where(Runner.IsLoadedHere).OrderBy(e => e.Name).Select(Runner.GetLoadedSource).ToList();
                limit = new HashSet<object>(selectedObjects.Select(e => e.WrappedObject), ReferenceEqualityComparer.Instance);
                break;

            case MsbMassEditScope.SelectedMap:
                sources = [Runner.GetLoadedSource(selectedMap)];
                break;

            case MsbMassEditScope.LoadedMaps:
                sources = Runner.GetLoadedSources();
                break;

            default:
                sources = Runner.GetGlobalSources();
                break;
        }

        // The inputs a scope hides are left out
        var filter = new MsbMassEditFilter
        {
            Map = Scope is MsbMassEditScope.AllMaps ? MapText.Trim() : "",
            Category = Category,
            Type = TypeText.Trim(),
            Name = NameText.Trim(),
            Conditions = Conditions.Select(e => e.ToCondition()).Where(e => !e.IsEmpty).ToList(),
            Exact = Exact,
            IgnoreDummies = IgnoreDummies,
            LimitObjects = limit
        };

        Results = new();
        ResultMaps = new();
        ResultMap = selectedMap;
        ResultsAcrossMaps = Scope is not MsbMassEditScope.SelectedMap;
        ResultScope = Scope;

        FoundVersion = InputsVersion;
        FoundMap = selectedMap;
        FoundObjects = selectedObjects.Select(e => e.WrappedObject).ToArray();

        // Every match is kept, as the edit is applied to all of them
        Run = Runner.Start(filter, sources, 0);
    }

    /// <summary>
    /// Whether the matches no longer show what the inputs ask for: the inputs have changed, or the selection that the
    /// scope covers has
    /// </summary>
    private bool IsStale()
    {
        if (FoundVersion != InputsVersion)
            return true;

        return ResultScope switch
        {
            MsbMassEditScope.SelectedObjects =>
                !GetSelectedObjects().Select(e => e.WrappedObject).SequenceEqual(FoundObjects),
            MsbMassEditScope.SelectedMap => GetSelectedMap() != FoundMap,
            _ => false
        };
    }
    #endregion

    #region Results
    /// <summary>
    /// Puts the map in front of a label when the matches can span maps
    /// </summary>
    private string PrefixLabel(MsbQueryMatch match, string label)
    {
        return ResultsAcrossMaps ? $"{match.MapID}  {label}" : label;
    }

    /// <summary>
    /// How many objects match, and in how many maps
    /// </summary>
    private string GetMatchCountText()
    {
        var text = LOC.Get("MAP_MassEditTool_Matches", Results.Count);

        if (ResultsAcrossMaps)
        {
            text = $"{text} {LOC.Get("MAP_MassEditTool_Matches_Maps", ResultMaps.Count)}";
        }

        return text;
    }

    /// <summary>
    /// Picks up what has been found so far
    /// </summary>
    private void PickUpResults()
    {
        var run = Run;

        if (run == null)
            return;

        while (run.Found.TryDequeue(out var match))
        {
            if (Results == null)
                continue;

            Results.Add(new ResultRow(match, PrefixLabel(match, match.Label)));
            ResultMaps.Add(match.MapID);
        }
    }

    /// <summary>
    /// Shows how far the find has got
    /// </summary>
    private void DisplayProgress()
    {
        var run = Run;

        if (run == null || run.Done)
            return;

        // Progress
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SearchTool_Progress_Header"),
            LOC.Get("MAP_SearchTool_Progress_Header_TT"));

        var total = Math.Max(run.Total, 1);
        var fraction = Math.Clamp((float)run.Progress / total, 0, 1);

        ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get("MAP_SearchTool_Progress", run.Progress, run.Total));

        GUI.WrappedText(LOC.Get("MAP_SearchTool_Searching", run.Status));

        GUI.MultiButtonInput("msbMassEditProgressActions",
            "msbMassEditCancel",
            LOC.Get("MAP_SearchTool_Cancel"),
            LOC.Get("MAP_SearchTool_Cancel_TT"),
            CancelRun);
    }

    private void DisplayResults()
    {
        if (Results == null)
            return;

        // Matches
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("MAP_MassEditTool_Matches_Header"),
            LOC.Get("MAP_MassEditTool_Matches_Header_TT"),
            ref ShowMatchesList);

        var countText = GetMatchCountText();

        var filtered = GetFilteredResults();

        if (filtered.Count != Results.Count)
        {
            countText = $"{countText} {LOC.Get("MAP_SearchTool_Results_Filtered", filtered.Count)}";
        }

        GUI.WrappedText(countText);

        if (IsStale())
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MassEditTool_Matches_Stale"));
        }

        // The counts stay on show when the list is hidden
        if (Results.Count == 0 || !ShowMatchesList)
            return;

        EditorFilters.DisplayFramedListFilter("msbMassEditResultFilter", ref ResultFilter, ref ExactResultFilter);

        // The list is the last thing in its tab, so it takes the rest of the height
        ImGui.BeginChild("##msbMassEditResults", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        MsbQueryMatch jumpTarget = null;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var (match, label) = Results[index];

                if (ImGui.Button($"{Icons.LocationArrow}##msbMassEditJump{index}"))
                {
                    jumpTarget = match;
                }
                GUI.Tooltip(LOC.Get("MAP_SearchTool_Jump_TT"));

                ImGui.SameLine();

                if (ImGui.Selectable($"{label}##msbMassEditResult{index}", IsSelected(match)))
                {
                    jumpTarget = match;
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

    private bool IsSelected(MsbQueryMatch match)
    {
        return match.Entry.Entity != null && View.ViewportSelection.GetSelection().Contains(match.Entry.Entity);
    }

    /// <summary>
    /// The positions of the results that match the filter. The results arrive while the find runs,
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

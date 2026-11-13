using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public enum ModelSearchMode
{
    /// <summary>
    /// Searches the loaded model, or the models of its container
    /// </summary>
    Local,

    /// <summary>
    /// Searches every model of the project
    /// </summary>
    Global
}

/// <summary>
/// Finds models, and the dummies, materials, bones and meshes within them, by their names, fields and values. Each
/// result jumps to what it found, loading the model and selecting the object. The matching is done by
/// <see cref="ModelQueryRunner"/>, in the background, as a global search reads every model of the project.
/// Follows the Time Act Editor's search tool.
/// </summary>
public class ModelSearchTool
{
    public ModelEditorView View;
    public ProjectEntry Project;
    public ModelSearchMode Mode;

    private enum SearchScope
    {
        CurrentModel,
        AllModels
    }

    // The results stop here, as a value such as 0 is found in nearly every object
    private const int MaxResults = 10000;

    private readonly string Id;
    private readonly ModelQueryRunner Runner;

    private SearchScope Scope = SearchScope.CurrentModel;

    // Global: the model list to search, or null for all of them
    private ModelListType? ListFilter = null;

    private string ModelText = "";
    private string ObjectText = "";
    private string FieldText = "";
    private string ValueText = "";
    private string Value2Text = "";
    private ModelQueryComparison Comparison = ModelQueryComparison.Contains;
    private bool Exact = false;

    private ModelQueryRun Run;

    // What the results were found with, so they can be handed to the mass edit
    private ModelQuery ResultQuery;
    private ModelMassEditScope ResultScope;
    private ModelListType? ResultListFilter;

    // Each result with the label it is listed under, which the filter matches
    private record ResultRow(ModelQueryMatch Result, string Label);

    private List<ResultRow> Results;

    // What a local search covered (the model, or its container), so the results are let go when it changes
    private object ResultSource;
    private bool ShowContainerInLabel;
    private bool ShowModelInLabel;

    private string ResultFilter = "";
    private bool ExactResultFilter = false;

    // Rebuilt only when the filter changes, and added to as results arrive
    private List<int> FilteredResults;
    private List<ResultRow> FilteredResultsFor;
    private string CachedResultFilter = "";
    private bool CachedExactResultFilter = false;
    private int CheckedResultCount;

    public ModelSearchTool(ModelEditorView view, ProjectEntry project, ModelSearchMode mode)
    {
        View = view;
        Project = project;
        Mode = mode;

        Id = mode is ModelSearchMode.Local ? "local" : "global";
        Runner = new ModelQueryRunner(view, project);
    }

    public void OnToolWindow()
    {
        var headerKey = Mode is ModelSearchMode.Local ? "MODEL_SearchTool_Local_Header" : "MODEL_SearchTool_Global_Header";

        if (!ToolSections.Header($"{LOC.Get(headerKey)}##modelSearchToolHeader_{Id}"))
            return;

        ImGui.BeginChild($"ModelSearchToolSection_{Id}", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get(Mode is ModelSearchMode.Local ? "MODEL_SearchTool_Local_Hint" : "MODEL_SearchTool_Global_Hint"));

        if (Mode is ModelSearchMode.Local)
        {
            var source = GetLocalSourceKey();

            // The results point into the model they were found in
            if (Results != null && ResultSource != source)
            {
                ClearResults();
            }

            if (source == null)
            {
                GUI.WrappedText(LOC.Get("MODEL_SearchTool_No_Model"));
                return;
            }
        }

        if (DisplayInputs())
        {
            StartSearch();
        }

        DisplayProgress();
        DisplayResults();
    }

    /// <summary>
    /// What a local search covers: the loaded model, or the container it is in
    /// </summary>
    private object GetLocalSourceKey()
    {
        var wrapper = View.Selection.SelectedModelWrapper;

        if (wrapper == null)
            return null;

        return Scope is SearchScope.AllModels ? wrapper.Parent : wrapper;
    }

    #region Inputs
    private bool DisplayInputs()
    {
        var search = false;

        GUI.Spacer();

        if (Mode is ModelSearchMode.Local)
        {
            // Scope: the loaded model, or every model of its container (a part or DS2 map holds several)
            GUI.SimpleHeader(
                LOC.Get("MODEL_SearchTool_Scope"),
                LOC.Get("MODEL_SearchTool_Scope_TT"));

            ImGui.SetNextItemWidth(-1);

            if (ImGui.BeginCombo($"##modelSearchScope_{Id}", GetScopeName(Scope)))
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
        else
        {
            // Lists: every model of the project, or one list of the Source List
            GUI.SimpleHeader(
                LOC.Get("MODEL_SearchTool_Lists"),
                LOC.Get("MODEL_SearchTool_Lists_TT"));

            ImGui.SetNextItemWidth(-1);

            if (ImGui.BeginCombo($"##modelSearchLists_{Id}", GetListName(ListFilter)))
            {
                if (ImGui.Selectable(GetListName(null), ListFilter == null))
                {
                    ListFilter = null;
                }

                foreach (var listType in ModelQueryRunner.SearchableListTypes)
                {
                    if (ImGui.Selectable(GetListName(listType), ListFilter == listType))
                    {
                        ListFilter = listType;
                    }
                }

                ImGui.EndCombo();
            }
        }

        search |= InputSection("MODEL_SearchTool_Model", $"##modelSearchModel_{Id}", ref ModelText);
        search |= InputSection("MODEL_SearchTool_Object", $"##modelSearchObject_{Id}", ref ObjectText);
        search |= InputSection("MODEL_SearchTool_Field", $"##modelSearchField_{Id}", ref FieldText);
        search |= DisplayValueInput();

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_SearchTool_Options_Header"),
            LOC.Get("MODEL_SearchTool_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MODEL_SearchTool_Exact")}##modelSearchExact_{Id}", ref Exact);
        GUI.Tooltip(LOC.Get("MODEL_SearchTool_Exact_TT"));

        var hasQuery = GetQuery().HasAny;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_SearchTool_Actions_Header"),
            LOC.Get("MODEL_SearchTool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput($"modelSearchActions_{Id}",
            $"modelSearchButton_{Id}",
            LOC.Get("MODEL_SearchTool_Search"),
            LOC.Get("MODEL_SearchTool_Search_TT"),
            () => search = true,
            hasQuery,

            $"modelSearchClear_{Id}",
            LOC.Get("MODEL_SearchTool_Clear"),
            LOC.Get("MODEL_SearchTool_Clear_TT"),
            ClearInputs,
            true);

        return search && hasQuery;
    }

    private void ClearInputs()
    {
        ModelText = "";
        ObjectText = "";
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
    /// The value, with how it is compared. Contains matches the text; the others compare numbers as numbers.
    /// Returns true when an input is left after it was changed.
    /// </summary>
    private bool DisplayValueInput()
    {
        var search = false;

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_SearchTool_Value"),
            LOC.Get("MODEL_SearchTool_Value_TT"));

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.35f);

        if (ImGui.BeginCombo($"##modelSearchComparison_{Id}", GetComparisonName(Comparison)))
        {
            foreach (var comparison in Enum.GetValues<ModelQueryComparison>())
            {
                if (ImGui.Selectable($"{GetComparisonName(comparison)}##modelSearchComparison_{Id}_{comparison}",
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

        if (Comparison is ModelQueryComparison.Between)
        {
            var width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2;

            ImGui.SetNextItemWidth(width);
            ImGui.InputTextWithHint($"##modelSearchValue_{Id}", LOC.Get("MODEL_SearchTool_Value_Low_Hint"), ref ValueText, 255);
            GUI.Tooltip(LOC.Get("MODEL_SearchTool_Value_Between_TT"));

            search |= ImGui.IsItemDeactivatedAfterEdit();

            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint($"##modelSearchValue2_{Id}", LOC.Get("MODEL_SearchTool_Value_High_Hint"), ref Value2Text, 255);
            GUI.Tooltip(LOC.Get("MODEL_SearchTool_Value_Between_TT"));

            search |= ImGui.IsItemDeactivatedAfterEdit();
        }
        else
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText($"##modelSearchValue_{Id}", ref ValueText, 255);
            GUI.Tooltip(LOC.Get("MODEL_SearchTool_Value_TT"));

            search |= ImGui.IsItemDeactivatedAfterEdit();
        }

        return search;
    }

    private static string GetScopeName(SearchScope scope) => scope switch
    {
        SearchScope.CurrentModel => LOC.Get("MODEL_SearchTool_Scope_Current"),
        SearchScope.AllModels => LOC.Get("MODEL_SearchTool_Scope_All"),
        _ => $"{scope}"
    };

    private static string GetListName(ModelListType? listType) => listType switch
    {
        null => LOC.Get("MODEL_SearchTool_Lists_All"),
        ModelListType.Character => LOC.Get("MODEL_ENUM_ModelListType_Characters"),
        ModelListType.Asset => LOC.Get("MODEL_ENUM_ModelListType_Assets"),
        ModelListType.Part => LOC.Get("MODEL_ENUM_ModelListType_Parts"),
        ModelListType.MapPiece => LOC.Get("MODEL_ENUM_ModelListType_MapPieces"),
        _ => $"{listType}"
    };

    public static string GetComparisonName(ModelQueryComparison comparison) => comparison switch
    {
        ModelQueryComparison.Contains => LOC.Get("MODEL_SearchTool_Comparison_Contains"),
        ModelQueryComparison.Equals => "=",
        ModelQueryComparison.NotEquals => "!=",
        ModelQueryComparison.Less => "<",
        ModelQueryComparison.LessOrEqual => "<=",
        ModelQueryComparison.Greater => ">",
        ModelQueryComparison.GreaterOrEqual => ">=",
        ModelQueryComparison.Between => LOC.Get("MODEL_SearchTool_Comparison_Between"),
        _ => $"{comparison}"
    };

    public static string GetComparisonTooltip(ModelQueryComparison comparison) => comparison switch
    {
        ModelQueryComparison.Contains => LOC.Get("MODEL_SearchTool_Comparison_Contains_TT"),
        ModelQueryComparison.Equals => LOC.Get("MODEL_SearchTool_Comparison_Equals_TT"),
        ModelQueryComparison.NotEquals => LOC.Get("MODEL_SearchTool_Comparison_Not_Equals_TT"),
        ModelQueryComparison.Between => LOC.Get("MODEL_SearchTool_Comparison_Between_TT"),
        _ => LOC.Get("MODEL_SearchTool_Comparison_Number_TT")
    };

    private ModelQuery GetQuery()
    {
        return new ModelQuery(ModelText.Trim(), ObjectText.Trim(), FieldText.Trim(), ValueText.Trim(), Exact,
            Comparison, Value2Text.Trim());
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
    /// Gathers the models to search on the main thread, where the editor's lists can be read safely, then hands them
    /// to a background task
    /// </summary>
    private void StartSearch()
    {
        ClearResults();

        List<ModelQuerySource> sources;

        if (Mode is ModelSearchMode.Local)
        {
            var wrapper = View.Selection.SelectedModelWrapper;

            if (wrapper == null)
                return;

            var models = Scope is SearchScope.AllModels ? wrapper.Parent.Models.ToList() : [wrapper];

            sources = [Runner.GetLocalSource(wrapper.Parent, models)];

            ShowContainerInLabel = false;
            ShowModelInLabel = Scope is SearchScope.AllModels;

            ResultSource = GetLocalSourceKey();
        }
        else
        {
            sources = Runner.GetGlobalSources(ListFilter);

            ShowContainerInLabel = true;
            ShowModelInLabel = true;
        }

        Results = new();

        // What the results were found with, so they can be handed to the mass edit
        ResultQuery = GetQuery();
        ResultScope = Mode is ModelSearchMode.Global ? ModelMassEditScope.AllModels
            : Scope is SearchScope.AllModels ? ModelMassEditScope.CurrentFile
            : ModelMassEditScope.CurrentModel;
        ResultListFilter = Mode is ModelSearchMode.Global ? ListFilter : null;

        // A local search counts its models, as there is only the one container
        Run = Runner.Start(GetQuery(), sources, Mode is ModelSearchMode.Local, MaxResults);
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
            LOC.Get("MODEL_SearchTool_Progress_Header"),
            LOC.Get("MODEL_SearchTool_Progress_Header_TT"));

        var total = Math.Max(run.Total, 1);
        var fraction = Math.Clamp((float)run.Progress / total, 0, 1);

        ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get("MODEL_SearchTool_Progress", run.Progress, run.Total));

        GUI.WrappedText(LOC.Get("MODEL_SearchTool_Searching", run.Status));

        GUI.MultiButtonInput($"modelSearchProgressActions_{Id}",
            $"modelSearchCancel_{Id}",
            LOC.Get("MODEL_SearchTool_Cancel"),
            LOC.Get("MODEL_SearchTool_Cancel_TT"),
            CancelRun);
    }

    private string GetResultLabel(ModelQueryMatch result)
    {
        var label = result.Label;

        // A match on the model itself is already labelled by its name
        var isModelMatch = result.Kind is ModelQueryObjectKind.Header && result.Label == result.ModelName;

        if (ShowModelInLabel && !isModelMatch)
            label = $"{result.ModelName}  {label}";

        // The container is only worth naming where it holds the model under another name (a part, a DS2 map)
        if (ShowContainerInLabel && !string.Equals(result.Container.Name, result.ModelName, StringComparison.OrdinalIgnoreCase))
            label = $"{result.Container.Name}  {label}";

        return label;
    }

    private void DisplayResults()
    {
        if (Results == null)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_SearchTool_Results_Header"),
            LOC.Get("MODEL_SearchTool_Results_Header_TT"));

        var filtered = GetFilteredResults();

        var truncated = Run is { Truncated: true };

        var countText = truncated
            ? LOC.Get("MODEL_SearchTool_Results_Truncated", MaxResults)
            : LOC.Get("MODEL_SearchTool_Results", Results.Count);

        if (filtered.Count != Results.Count)
        {
            countText = $"{countText} {LOC.Get("MODEL_SearchTool_Results_Filtered", filtered.Count)}";
        }

        GUI.WrappedText(countText);

        if (Results.Count == 0)
            return;

        // The results can be edited all at once, by finding them again in the mass edit
        if (ResultQuery != null)
        {
            GUI.MultiButtonInput($"modelSearchSendActions_{Id}",
                $"modelSearchSend_{Id}",
                LOC.Get("MODEL_SearchTool_Send_To_Mass_Edit"),
                LOC.Get("MODEL_SearchTool_Send_To_Mass_Edit_TT"),
                () => View.MassEditTool.LoadQuery(ResultQuery, ResultScope, ResultListFilter));
        }

        EditorFilters.DisplayFramedListFilter($"modelSearchResultFilter_{Id}", ref ResultFilter, ref ExactResultFilter);

        ImGui.BeginChild($"##modelSearchResults_{Id}", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        ModelQueryMatch jumpTarget = null;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var (result, label) = Results[index];

                if (ImGui.Button($"{Icons.LocationArrow}##modelSearchJump_{Id}{index}"))
                {
                    jumpTarget = result;
                }
                GUI.Tooltip(LOC.Get("MODEL_SearchTool_Jump_TT"));

                ImGui.SameLine();

                if (ImGui.Selectable($"{label}##modelSearchResult_{Id}{index}", ModelQueryRunner.IsSelected(View, result)))
                {
                    jumpTarget = result;
                }

                if (Mode is ModelSearchMode.Global)
                {
                    var alias = AliasHelper.GetAlias(Project, result.Container.Name, result.ListType);

                    if (!string.IsNullOrEmpty(alias))
                    {
                        GUI.Tooltip(alias);
                    }
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
    /// The positions of the results that match the filter. The results arrive while the search runs, so only the new
    /// ones are checked, unless the filter has changed.
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

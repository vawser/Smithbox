using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Finds dummies, materials, bones, meshes and model headers with a filter, then edits or deletes every match in one
/// step. The filter is matched by <see cref="ModelQueryRunner"/>, as the searches are. Follows the Time Act Editor's
/// mass edit. See Claude/Documentation/Model Editor Mass Edit Plan.md.
/// </summary>
public partial class ModelMassEditTool
{
    public ModelEditorView View;
    public ProjectEntry Project;

    private readonly ModelQueryRunner Runner;

    /// <summary>
    /// A condition as it is being edited. It is copied into a <see cref="ModelMassEditCondition"/> on Find.
    /// </summary>
    private class ConditionInput
    {
        public string Field = "";
        public ModelQueryComparison Comparison = ModelQueryComparison.Equals;
        public string Value = "";
        public string Value2 = "";

        public ModelMassEditCondition ToCondition() =>
            new(Field.Trim(), false, Comparison, Value.Trim(), Value2.Trim());
    }

    private ModelMassEditScope Scope = ModelMassEditScope.CurrentModel;
    private ModelListType? ListFilter = null;

    private string ModelText = "";
    private ModelQueryObjectKind? Kind = null;
    private string ObjectText = "";
    private List<ConditionInput> Conditions = [new()];
    private bool Exact = false;

    // Raised by every change to the inputs, so the matches can tell they no longer show what the inputs ask for
    private int InputsVersion;
    private int FoundVersion = -1;

    // What the scope pointed to when Find was pressed
    private ModelWrapper FoundModel;
    private ISelectable[] FoundSelection;

    private ModelQueryRun Run;

    private record ResultRow(ModelQueryMatch Match, string Label);

    private List<ResultRow> Results;
    private HashSet<(ModelContainerWrapper, string)> ResultModels;
    private bool ResultsGlobal;
    private bool ShowContainerInLabel;
    private bool ShowModelInLabel;

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

    public ModelMassEditTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Runner = new ModelQueryRunner(view, project);
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MODEL_MassEditTool_Header")}##modelMassEditToolHeader"))
            return;

        ImGui.BeginChild("ModelMassEditToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("MODEL_MassEditTool_Hint"));

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

        if (ImGui.BeginTabBar("##modelMassEditTabs"))
        {
            var findFlags = SelectFindTab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;
            SelectFindTab = false;

            // Find
            if (ImGui.BeginTabItem($"{LOC.Get("MODEL_MassEditTool_Tab_Find")}##modelMassEditTabFind", findFlags))
            {
                DisplayScope();
                DisplayFilter();

                if (DisplayFindActions())
                {
                    StartFind();
                }

                DisplayProgress();
                DisplayResults();

                ImGui.EndTabItem();
            }
            GUI.Tooltip(LOC.Get("MODEL_MassEditTool_Tab_Find_TT"));

            // Edit
            if (ImGui.BeginTabItem($"{LOC.Get("MODEL_MassEditTool_Tab_Edit")}##modelMassEditTabEdit"))
            {
                DisplayEdit();

                ImGui.EndTabItem();
            }
            GUI.Tooltip(LOC.Get("MODEL_MassEditTool_Tab_Edit_TT"));

            ImGui.EndTabBar();
        }
    }

    private void Changed()
    {
        InputsVersion++;
    }

    /// <summary>
    /// Takes over the inputs of a search, so its results can be edited all at once. The field and value become a
    /// condition, with an exact search's contains turned into equals. The tool is shown, and Find runs once it is
    /// next drawn.
    /// </summary>
    public void LoadQuery(ModelQuery query, ModelMassEditScope scope, ModelListType? listFilter)
    {
        Scope = scope;
        ListFilter = listFilter;

        ModelText = query.Model ?? "";
        Kind = null;
        ObjectText = query.Object ?? "";
        Exact = query.Exact;

        var condition = new ConditionInput();

        if (query.HasField || query.HasValue)
        {
            condition.Field = query.Field ?? "";
            condition.Comparison = query.Comparison is ModelQueryComparison.Contains
                ? query.Exact ? ModelQueryComparison.Equals : ModelQueryComparison.Contains
                : query.Comparison;
            condition.Value = query.Value ?? "";
            condition.Value2 = query.Value2 ?? "";
        }

        Conditions = [condition];

        Changed();
        ClearResults();

        CFG.Current.Interface_ModelEditor_Tool_MassEdit = true;

        PendingFind = true;
        SelectFindTab = true;
    }

    #region Scope
    private void DisplayScope()
    {
        // Scope
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_MassEditTool_Scope_Header"),
            LOC.Get("MODEL_MassEditTool_Scope_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##modelMassEditScope", GetScopeName(Scope)))
        {
            foreach (var scope in Enum.GetValues<ModelMassEditScope>())
            {
                if (ImGui.Selectable($"{GetScopeName(scope)}##modelMassEditScope{scope}", Scope == scope))
                {
                    Scope = scope;
                    Changed();
                }

                GUI.Tooltip(GetScopeTooltip(scope));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(GetScopeTooltip(Scope));

        if (Scope is ModelMassEditScope.AllModels)
        {
            ImGui.SetNextItemWidth(-1);

            if (ImGui.BeginCombo("##modelMassEditLists", GetListName(ListFilter)))
            {
                if (ImGui.Selectable($"{GetListName(null)}##modelMassEditListAll", ListFilter == null))
                {
                    ListFilter = null;
                    Changed();
                }

                foreach (var listType in ModelQueryRunner.SearchableListTypes)
                {
                    if (ImGui.Selectable($"{GetListName(listType)}##modelMassEditList{listType}", ListFilter == listType))
                    {
                        ListFilter = listType;
                        Changed();
                    }
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("MODEL_SearchTool_Lists_TT"));
        }

        var unavailable = GetScopeUnavailableReason();

        if (unavailable != null)
        {
            GUI.WrappedText(unavailable);
        }
    }

    /// <summary>
    /// The scope needs the selection to point at what it covers. Gives the reason it can't be used, or null.
    /// </summary>
    private string GetScopeUnavailableReason()
    {
        if (Scope is ModelMassEditScope.AllModels)
            return null;

        if (View.Selection.SelectedModelWrapper?.Container == null)
            return LOC.Get("MODEL_MassEditTool_No_Model");

        if (Scope is ModelMassEditScope.SelectedObjects && GetSelectedObjects().Count == 0)
            return LOC.Get("MODEL_MassEditTool_Scope_No_Selection");

        return null;
    }

    /// <summary>
    /// The FLVER objects selected in the loaded model; the model root stands for its header
    /// </summary>
    private HashSet<object> GetSelectedObjects()
    {
        var result = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var wrapper = View.Selection.SelectedModelWrapper;

        if (wrapper?.Container == null)
            return result;

        foreach (var selected in View.ViewportSelection.GetSelection())
        {
            if (selected is not Entity entity)
                continue;

            if (entity == wrapper.Container.RootObject)
            {
                if (wrapper.FLVER?.Header != null)
                    result.Add(wrapper.FLVER.Header);
            }
            else if (entity.WrappedObject is FLVER.Dummy or FLVER2.Material or FLVER.Node or FLVER2.Mesh)
            {
                result.Add(entity.WrappedObject);
            }
        }

        return result;
    }

    private static string GetScopeName(ModelMassEditScope scope) => scope switch
    {
        ModelMassEditScope.SelectedObjects => LOC.Get("MODEL_MassEditTool_Scope_Selected"),
        ModelMassEditScope.CurrentModel => LOC.Get("MODEL_MassEditTool_Scope_Current_Model"),
        ModelMassEditScope.CurrentFile => LOC.Get("MODEL_MassEditTool_Scope_Current_File"),
        ModelMassEditScope.AllModels => LOC.Get("MODEL_MassEditTool_Scope_All_Models"),
        _ => $"{scope}"
    };

    private static string GetScopeTooltip(ModelMassEditScope scope) => scope switch
    {
        ModelMassEditScope.SelectedObjects => LOC.Get("MODEL_MassEditTool_Scope_Selected_TT"),
        ModelMassEditScope.CurrentModel => LOC.Get("MODEL_MassEditTool_Scope_Current_Model_TT"),
        ModelMassEditScope.CurrentFile => LOC.Get("MODEL_MassEditTool_Scope_Current_File_TT"),
        ModelMassEditScope.AllModels => LOC.Get("MODEL_MassEditTool_Scope_All_Models_TT"),
        _ => ""
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
    #endregion

    #region Filter
    private void DisplayFilter()
    {
        // Model
        if (Scope is ModelMassEditScope.CurrentFile or ModelMassEditScope.AllModels)
        {
            TextSection("MODEL_MassEditTool_Model", "##modelMassEditModel", ref ModelText);
        }

        // Object
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_MassEditTool_Object"),
            LOC.Get("MODEL_MassEditTool_Object_TT"));

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.35f);

        if (ImGui.BeginCombo("##modelMassEditKind", GetKindName(Kind)))
        {
            if (ImGui.Selectable($"{GetKindName(null)}##modelMassEditKindAny", Kind == null))
            {
                Kind = null;
                Changed();
            }

            foreach (var kind in Enum.GetValues<ModelQueryObjectKind>())
            {
                if (ImGui.Selectable($"{GetKindName(kind)}##modelMassEditKind{kind}", Kind == kind))
                {
                    Kind = kind;
                    Changed();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MODEL_MassEditTool_Object_TT"));

        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputTextWithHint("##modelMassEditObjectName", LOC.Get("MODEL_MassEditTool_Object_Name_Hint"), ref ObjectText, 255))
        {
            Changed();
        }
        GUI.Tooltip(LOC.Get("MODEL_MassEditTool_Object_TT"));

        DisplayConditions();

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_MassEditTool_Options_Header"),
            LOC.Get("MODEL_MassEditTool_Options_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MODEL_SearchTool_Exact")}##modelMassEditExact", ref Exact))
        {
            Changed();
        }
        GUI.Tooltip(LOC.Get("MODEL_SearchTool_Exact_TT"));
    }

    private void TextSection(string nameKey, string id, ref string text)
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get(nameKey), LOC.Get($"{nameKey}_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputText(id, ref text, 255))
        {
            Changed();
        }
        GUI.Tooltip(LOC.Get($"{nameKey}_TT"));
    }

    private static string GetKindName(ModelQueryObjectKind? kind) =>
        kind == null ? LOC.Get("MODEL_MassEditTool_Kind_Any") : ModelQueryMatcher.GetKindName(kind.Value);

    /// <summary>
    /// The conditions: each must be met by a field of the object
    /// </summary>
    private void DisplayConditions()
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_MassEditTool_Conditions_Header"),
            LOC.Get("MODEL_MassEditTool_Conditions_Header_TT"));

        ConditionInput removed = null;

        for (int i = 0; i < Conditions.Count; i++)
        {
            var condition = Conditions[i];

            if (i > 0)
            {
                ImGui.Separator();
            }

            var removeWidth = DPI.IconButtonSize.X + ImGui.GetStyle().ItemSpacing.X;

            ImGui.SetNextItemWidth(-removeWidth);

            if (ImGui.InputTextWithHint($"##modelMassEditConditionField{i}", LOC.Get("MODEL_MassEditTool_Condition_Field_Hint"),
                ref condition.Field, 255))
            {
                Changed();
            }
            GUI.Tooltip(LOC.Get("MODEL_MassEditTool_Condition_Field_TT"));

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Minus}##modelMassEditConditionRemove{i}", DPI.IconButtonSize))
            {
                removed = condition;
            }
            GUI.Tooltip(LOC.Get("MODEL_MassEditTool_Condition_Remove_TT"));

            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.35f);

            if (ImGui.BeginCombo($"##modelMassEditConditionComparison{i}", ModelSearchTool.GetComparisonName(condition.Comparison)))
            {
                foreach (var comparison in Enum.GetValues<ModelQueryComparison>())
                {
                    if (ImGui.Selectable($"{ModelSearchTool.GetComparisonName(comparison)}##modelMassEditComparison{i}_{comparison}",
                        condition.Comparison == comparison))
                    {
                        condition.Comparison = comparison;
                        Changed();
                    }
                    GUI.Tooltip(ModelSearchTool.GetComparisonTooltip(comparison));
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(ModelSearchTool.GetComparisonTooltip(condition.Comparison));

            ImGui.SameLine();

            if (condition.Comparison is ModelQueryComparison.Between)
            {
                var width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2;

                ImGui.SetNextItemWidth(width);

                if (ImGui.InputTextWithHint($"##modelMassEditConditionValue{i}", LOC.Get("MODEL_SearchTool_Value_Low_Hint"), ref condition.Value, 255))
                    Changed();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);

                if (ImGui.InputTextWithHint($"##modelMassEditConditionValue2{i}", LOC.Get("MODEL_SearchTool_Value_High_Hint"), ref condition.Value2, 255))
                    Changed();
            }
            else
            {
                ImGui.SetNextItemWidth(-1);

                if (ImGui.InputTextWithHint($"##modelMassEditConditionValue{i}", LOC.Get("MODEL_MassEditTool_Condition_Value_Hint"), ref condition.Value, 255))
                    Changed();
            }
            GUI.Tooltip(LOC.Get("MODEL_SearchTool_Value_TT"));
        }

        if (removed != null)
        {
            Conditions.Remove(removed);
            Changed();
        }

        GUI.MultiButtonInput("modelMassEditConditionActions",
            "modelMassEditConditionAdd",
            LOC.Get("MODEL_MassEditTool_Condition_Add"),
            LOC.Get("MODEL_MassEditTool_Condition_Add_TT"),
            () =>
            {
                Conditions.Add(new());
                Changed();
            });
    }
    #endregion

    #region Find
    private bool DisplayFindActions()
    {
        var find = false;
        var canFind = GetScopeUnavailableReason() == null;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_MassEditTool_Actions_Header"),
            LOC.Get("MODEL_MassEditTool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("modelMassEditActions",
            "modelMassEditFind",
            LOC.Get("MODEL_MassEditTool_Find"),
            LOC.Get("MODEL_MassEditTool_Find_TT"),
            () => find = true,
            canFind,

            "modelMassEditClear",
            LOC.Get("MODEL_SearchTool_Clear"),
            LOC.Get("MODEL_SearchTool_Clear_TT"),
            ClearInputs,
            true);

        return find && canFind;
    }

    private void ClearInputs()
    {
        ModelText = "";
        Kind = null;
        ObjectText = "";
        Conditions = [new()];

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
        ResultModels = null;
        ResultsGlobal = false;
        FoundVersion = -1;

        ClearPlan();
    }

    /// <summary>
    /// Takes what the scope covers from the selection on the main thread, then hands the filter to a background task
    /// </summary>
    private void StartFind()
    {
        ClearResults();

        var wrapper = View.Selection.SelectedModelWrapper;
        var global = Scope is ModelMassEditScope.AllModels;

        List<ModelQuerySource> sources = Scope switch
        {
            ModelMassEditScope.AllModels => Runner.GetGlobalSources(ListFilter),
            ModelMassEditScope.CurrentFile => [Runner.GetLocalSource(wrapper.Parent, wrapper.Parent.Models.ToList())],
            _ => [Runner.GetLocalSource(wrapper.Parent, [wrapper])]
        };

        var filter = new ModelMassEditFilter
        {
            Model = Scope is ModelMassEditScope.CurrentFile or ModelMassEditScope.AllModels ? ModelText.Trim() : "",
            Kind = Kind,
            ObjectText = ObjectText.Trim(),
            Conditions = Conditions.Select(e => e.ToCondition()).ToList(),
            Exact = Exact,
            LimitObjects = Scope is ModelMassEditScope.SelectedObjects ? GetSelectedObjects() : null
        };

        Results = new();
        ResultModels = new();
        ResultsGlobal = global;
        ShowContainerInLabel = global;
        ShowModelInLabel = Scope is ModelMassEditScope.CurrentFile or ModelMassEditScope.AllModels;

        FoundVersion = InputsVersion;
        FoundModel = wrapper;
        FoundSelection = View.ViewportSelection.GetSelection().ToArray();

        // Every match is kept, as the edit is applied to all of them
        Run = Runner.Start(filter, sources, !global, 0);
    }

    /// <summary>
    /// Whether the matches no longer show what the inputs ask for: the inputs have changed, or the selection that
    /// the scope covers has
    /// </summary>
    private bool IsStale()
    {
        if (FoundVersion != InputsVersion)
            return true;

        return Scope switch
        {
            ModelMassEditScope.SelectedObjects => View.Selection.SelectedModelWrapper != FoundModel ||
                !View.ViewportSelection.GetSelection().SetEquals(FoundSelection),
            ModelMassEditScope.CurrentModel => View.Selection.SelectedModelWrapper != FoundModel,
            ModelMassEditScope.CurrentFile => View.Selection.SelectedModelWrapper?.Parent != FoundModel?.Parent,
            _ => false
        };
    }
    #endregion

    #region Results
    private string PrefixLabel(ModelQueryMatch match, string label)
    {
        if (ShowModelInLabel)
            label = $"{match.ModelName}  {label}";

        // The file is only worth naming where it holds the model under another name (a part, a DS2 map)
        if (ShowContainerInLabel && !string.Equals(match.Container.Name, match.ModelName, StringComparison.OrdinalIgnoreCase))
            label = $"{match.Container.Name}  {label}";

        return label;
    }

    private string GetMatchCountText()
    {
        return LOC.Get("MODEL_MassEditTool_Matches", Results.Count, ResultModels.Count);
    }

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
            ResultModels.Add((match.Container, match.ModelName.ToLowerInvariant()));
        }
    }

    private void DisplayProgress()
    {
        var run = Run;

        if (run == null || run.Done)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_SearchTool_Progress_Header"),
            LOC.Get("MODEL_SearchTool_Progress_Header_TT"));

        var total = Math.Max(run.Total, 1);
        var fraction = Math.Clamp((float)run.Progress / total, 0, 1);

        ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get("MODEL_SearchTool_Progress", run.Progress, run.Total));

        GUI.WrappedText(LOC.Get("MODEL_SearchTool_Searching", run.Status));

        GUI.MultiButtonInput("modelMassEditProgressActions",
            "modelMassEditCancel",
            LOC.Get("MODEL_SearchTool_Cancel"),
            LOC.Get("MODEL_SearchTool_Cancel_TT"),
            CancelRun);
    }

    private void DisplayResults()
    {
        if (Results == null)
            return;

        // Matches
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("MODEL_MassEditTool_Matches_Header"),
            LOC.Get("MODEL_MassEditTool_Matches_Header_TT"),
            ref ShowMatchesList);

        var countText = GetMatchCountText();
        var filtered = GetFilteredResults();

        if (filtered.Count != Results.Count)
        {
            countText = $"{countText} {LOC.Get("MODEL_SearchTool_Results_Filtered", filtered.Count)}";
        }

        GUI.WrappedText(countText);

        if (IsStale())
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_MassEditTool_Matches_Stale"));
        }

        if (Results.Count == 0 || !ShowMatchesList)
            return;

        DisplayMatchList("modelMassEditResults", filtered);
    }

    /// <summary>
    /// The matches, filtered, each jumping to its object. The list takes the rest of the height.
    /// </summary>
    private void DisplayMatchList(string id, List<int> filtered)
    {
        EditorFilters.DisplayFramedListFilter($"{id}Filter", ref ResultFilter, ref ExactResultFilter);

        ImGui.BeginChild($"##{id}", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        ModelQueryMatch jumpTarget = null;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var (match, label) = Results[index];

                if (ImGui.Button($"{Icons.LocationArrow}##{id}Jump{index}"))
                {
                    jumpTarget = match;
                }
                GUI.Tooltip(LOC.Get("MODEL_SearchTool_Jump_TT"));

                ImGui.SameLine();

                if (ImGui.Selectable($"{label}##{id}{index}", ModelQueryRunner.IsSelected(View, match)))
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

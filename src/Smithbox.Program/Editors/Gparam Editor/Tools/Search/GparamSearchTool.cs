using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

public enum GparamSearchMode
{
    /// <summary>
    /// Searches the selected file
    /// </summary>
    Local,

    /// <summary>
    /// Searches every file of the project
    /// </summary>
    Global
}

/// <summary>
/// Finds groups, fields and values by their names, annotations and values. Each result jumps to what it found,
/// selecting the file, group, field and value. The matching is done by <see cref="GparamQueryRunner"/>,
/// in the background, as a global search reads every file of the project.
/// </summary>
public class GparamSearchTool
{
    public GparamEditorView View;
    public ProjectEntry Project;
    public GparamSearchMode Mode;

    private enum SearchScope
    {
        SelectedFile,
        SelectedGroup,
        SelectedField
    }

    // The results stop here, as a value such as 0 is found in nearly every field
    private const int MaxResults = 10000;

    private readonly string Id;
    private readonly GparamQueryRunner Runner;

    private SearchScope Scope = SearchScope.SelectedFile;
    private string FileText = "";
    private string GroupText = "";
    private string FieldText = "";
    private string ValueText = "";
    private string Value2Text = "";
    private GparamQueryComparison Comparison = GparamQueryComparison.Contains;
    private GparamQueryValueTarget ValueTarget = GparamQueryValueTarget.Value;
    private bool Exact = false;
    private bool UniqueValues = false;

    // What the results were found with, so they can be handed to the mass edit
    private GparamQuery ResultQuery;
    private GparamMassEditScope ResultScope;

    private GparamQueryRun Run;

    // Each result with the label it is listed under, which the filter matches
    private record ResultRow(GparamQueryMatch Result, string Label);

    private List<ResultRow> Results;
    private FileDictionaryEntry ResultSource;
    private bool ShowFileInLabel;

    private string ResultFilter = "";
    private bool ExactResultFilter = false;

    // Rebuilt only when the filter changes, and added to as results arrive
    private List<int> FilteredResults;
    private List<ResultRow> FilteredResultsFor;
    private string CachedResultFilter = "";
    private bool CachedExactResultFilter = false;
    private bool CachedUniqueValues = false;
    private HashSet<string> SeenValues = new();
    private int CheckedResultCount;

    public GparamSearchTool(GparamEditorView view, ProjectEntry project, GparamSearchMode mode)
    {
        View = view;
        Project = project;
        Mode = mode;

        Id = mode is GparamSearchMode.Local ? "local" : "global";
        Runner = new GparamQueryRunner(view, project);
    }

    public void OnToolWindow()
    {
        var headerKey = Mode is GparamSearchMode.Local ? "GPARAM_SearchTool_Local_Header" : "GPARAM_SearchTool_Global_Header";

        if (!ToolSections.Header($"{LOC.Get(headerKey)}##gparamSearchToolHeader_{Id}"))
            return;

        ImGui.BeginChild($"GparamSearchToolSection_{Id}", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get(Mode is GparamSearchMode.Local ? "GPARAM_SearchTool_Local_Hint" : "GPARAM_SearchTool_Global_Hint"));

        var selection = View.Selection;
        var source = selection.IsFileSelected() ? selection.SelectedFileEntry : null;

        if (Mode is GparamSearchMode.Local)
        {
            // The results point into the file they were found in
            if (Results != null && ResultSource != source)
            {
                ClearResults();
            }

            if (source == null)
            {
                GUI.WrappedText(LOC.Get("GPARAM_SearchTool_No_File"));
                return;
            }
        }

        if (DisplayInputs())
        {
            StartSearch(source);
        }

        DisplayProgress();
        DisplayResults();
    }

    #region Targeting
    /// <summary>
    /// Fills an input from a list's context menu, and shows the tool so it can be seen
    /// </summary>
    public void TargetFile(string filename)
    {
        FileText = filename;
        Show();
    }

    public void TargetGroup(string groupKey)
    {
        GroupText = groupKey;
        Show();
    }

    public void TargetField(string fieldKey)
    {
        FieldText = fieldKey;
        Show();
    }

    /// <summary>
    /// Finds the value with this ID, in the field and group given beside it
    /// </summary>
    public void TargetValue(string groupKey, string fieldKey, int valueId)
    {
        GroupText = groupKey;
        FieldText = fieldKey;
        ValueText = $"{valueId}";
        Value2Text = "";
        ValueTarget = GparamQueryValueTarget.ID;
        Comparison = GparamQueryComparison.Equals;
        Show();
    }

    private void Show()
    {
        if (Mode is GparamSearchMode.Local)
            CFG.Current.Interface_GparamEditor_Tool_LocalSearch = true;
        else
            CFG.Current.Interface_GparamEditor_Tool_GlobalSearch = true;
    }
    #endregion

    #region Inputs
    private bool DisplayInputs()
    {
        var search = false;

        // Scope: a global search covers every file
        if (Mode is GparamSearchMode.Local)
        {
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("GPARAM_SearchTool_Scope"),
                LOC.Get("GPARAM_SearchTool_Scope_TT"));

            ImGui.SetNextItemWidth(-1);

            if (ImGui.BeginCombo($"##gparamSearchScope_{Id}", GetScopeName(Scope)))
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

            if (!IsScopeSelected())
            {
                GUI.WrappedText(LOC.Get(Scope is SearchScope.SelectedGroup
                    ? "GPARAM_SearchTool_No_Group" : "GPARAM_SearchTool_No_Field"));
            }
        }
        else
        {
            search |= InputSection("GPARAM_SearchTool_File", $"##gparamSearchFile_{Id}", ref FileText);
        }

        search |= InputSection("GPARAM_SearchTool_Group", $"##gparamSearchGroup_{Id}", ref GroupText);
        search |= InputSection("GPARAM_SearchTool_Field", $"##gparamSearchField_{Id}", ref FieldText);
        search |= DisplayValueInput();

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("GPARAM_SearchTool_Options_Header"),
            LOC.Get("GPARAM_SearchTool_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("GPARAM_SearchTool_Exact")}##gparamSearchExact_{Id}", ref Exact);
        GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Exact_TT"));

        ImGui.Checkbox($"{LOC.Get("GPARAM_SearchTool_Unique_Values")}##gparamSearchUnique_{Id}", ref UniqueValues);
        GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Unique_Values_TT"));

        var query = GetQuery();
        var hasQuery = query.HasAny && IsScopeSelected();

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("GPARAM_SearchTool_Actions_Header"),
            LOC.Get("GPARAM_SearchTool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput($"gparamSearchActions_{Id}",
            $"gparamSearchButton_{Id}",
            LOC.Get("GPARAM_SearchTool_Search"),
            LOC.Get("GPARAM_SearchTool_Search_TT"),
            () => search = true,
            hasQuery,

            $"gparamSearchClear_{Id}",
            LOC.Get("GPARAM_SearchTool_Clear"),
            LOC.Get("GPARAM_SearchTool_Clear_TT"),
            ClearInputs,
            true);

        return search && hasQuery;
    }

    /// <summary>
    /// Whether what the local scope searches is selected
    /// </summary>
    private bool IsScopeSelected()
    {
        if (Mode is GparamSearchMode.Global)
            return true;

        return Scope switch
        {
            SearchScope.SelectedGroup => View.Selection.IsGparamGroupSelected(),
            SearchScope.SelectedField => View.Selection.IsGparamGroupSelected() && View.Selection.IsGparamFieldSelected(),
            _ => true
        };
    }

    private void ClearInputs()
    {
        FileText = "";
        GroupText = "";
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
    /// The value, with what part of the row it is matched against and how it is compared.
    /// Returns true when an input is left after it was changed.
    /// </summary>
    private bool DisplayValueInput()
    {
        var search = false;

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("GPARAM_SearchTool_Value"),
            LOC.Get("GPARAM_SearchTool_Value_TT"));

        var comboWidth = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) * 0.25f;

        // What is matched: the value, its ID or its time of day
        ImGui.SetNextItemWidth(comboWidth);

        if (ImGui.BeginCombo($"##gparamSearchValueTarget_{Id}", GparamQueryRunner.GetValueTargetName(ValueTarget)))
        {
            foreach (var target in Enum.GetValues<GparamQueryValueTarget>())
            {
                if (ImGui.Selectable($"{GparamQueryRunner.GetValueTargetName(target)}##gparamSearchValueTarget_{Id}_{target}",
                    ValueTarget == target))
                {
                    ValueTarget = target;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Value_Target_TT"));

        ImGui.SameLine();

        // How it is compared
        ImGui.SetNextItemWidth(comboWidth);

        if (ImGui.BeginCombo($"##gparamSearchComparison_{Id}", GetComparisonName(Comparison)))
        {
            foreach (var comparison in Enum.GetValues<GparamQueryComparison>())
            {
                if (ImGui.Selectable($"{GetComparisonName(comparison)}##gparamSearchComparison_{Id}_{comparison}",
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

        if (Comparison is GparamQueryComparison.Between)
        {
            var width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2;

            ImGui.SetNextItemWidth(width);
            ImGui.InputTextWithHint($"##gparamSearchValue_{Id}", LOC.Get("GPARAM_SearchTool_Value_Low_Hint"), ref ValueText, 255);
            GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Value_Between_TT"));

            search |= ImGui.IsItemDeactivatedAfterEdit();

            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint($"##gparamSearchValue2_{Id}", LOC.Get("GPARAM_SearchTool_Value_High_Hint"), ref Value2Text, 255);
            GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Value_Between_TT"));

            search |= ImGui.IsItemDeactivatedAfterEdit();
        }
        else
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText($"##gparamSearchValue_{Id}", ref ValueText, 255);
            GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Value_TT"));

            search |= ImGui.IsItemDeactivatedAfterEdit();
        }

        return search;
    }

    private static string GetScopeName(SearchScope scope) => scope switch
    {
        SearchScope.SelectedFile => LOC.Get("GPARAM_SearchTool_Scope_File"),
        SearchScope.SelectedGroup => LOC.Get("GPARAM_SearchTool_Scope_Group"),
        SearchScope.SelectedField => LOC.Get("GPARAM_SearchTool_Scope_Field"),
        _ => $"{scope}"
    };

    public static string GetComparisonName(GparamQueryComparison comparison) => comparison switch
    {
        GparamQueryComparison.Contains => LOC.Get("GPARAM_SearchTool_Comparison_Contains"),
        GparamQueryComparison.Equals => "=",
        GparamQueryComparison.NotEquals => "!=",
        GparamQueryComparison.Less => "<",
        GparamQueryComparison.LessOrEqual => "<=",
        GparamQueryComparison.Greater => ">",
        GparamQueryComparison.GreaterOrEqual => ">=",
        GparamQueryComparison.Between => LOC.Get("GPARAM_SearchTool_Comparison_Between"),
        _ => $"{comparison}"
    };

    public static string GetComparisonTooltip(GparamQueryComparison comparison) => comparison switch
    {
        GparamQueryComparison.Contains => LOC.Get("GPARAM_SearchTool_Comparison_Contains_TT"),
        GparamQueryComparison.Equals => LOC.Get("GPARAM_SearchTool_Comparison_Equals_TT"),
        GparamQueryComparison.NotEquals => LOC.Get("GPARAM_SearchTool_Comparison_Not_Equals_TT"),
        GparamQueryComparison.Less => LOC.Get("GPARAM_SearchTool_Comparison_Less_TT"),
        GparamQueryComparison.LessOrEqual => LOC.Get("GPARAM_SearchTool_Comparison_Less_Or_Equal_TT"),
        GparamQueryComparison.Greater => LOC.Get("GPARAM_SearchTool_Comparison_Greater_TT"),
        GparamQueryComparison.GreaterOrEqual => LOC.Get("GPARAM_SearchTool_Comparison_Greater_Or_Equal_TT"),
        GparamQueryComparison.Between => LOC.Get("GPARAM_SearchTool_Comparison_Between_TT"),
        _ => ""
    };

    private GparamQuery GetQuery()
    {
        var file = Mode is GparamSearchMode.Global ? FileText.Trim() : "";

        return new GparamQuery(file, GroupText.Trim(), FieldText.Trim(), ValueText.Trim(), Exact,
            Comparison, Value2Text.Trim(), ValueTarget);
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
    /// Gathers the files to search on the main thread, where the editor's state can be read safely,
    /// then hands them to a background task
    /// </summary>
    private void StartSearch(FileDictionaryEntry selectedFile)
    {
        ClearResults();

        var selection = View.Selection;
        var query = GetQuery();

        List<GparamQuerySource> sources;

        if (Mode is GparamSearchMode.Local)
        {
            string groupKey = null;
            string fieldKey = null;

            // The search is only offered once what the scope searches is selected
            if (Scope is SearchScope.SelectedGroup or SearchScope.SelectedField)
            {
                groupKey = selection._selectedParamGroupKey;
            }

            if (Scope is SearchScope.SelectedField)
            {
                fieldKey = selection._selectedParamFieldKey;
            }

            sources = [Runner.GetLocalSource(selectedFile, selection.GetSelectedGparam(), groupKey, fieldKey)];

            ShowFileInLabel = false;
        }
        else
        {
            sources = Runner.GetGlobalSources();

            ShowFileInLabel = true;
        }

        Results = new();
        ResultSource = selectedFile;

        ResultQuery = query;
        ResultScope = Mode is GparamSearchMode.Global ? GparamMassEditScope.AllFiles : Scope switch
        {
            SearchScope.SelectedGroup => GparamMassEditScope.SelectedGroup,
            SearchScope.SelectedField => GparamMassEditScope.SelectedField,
            _ => GparamMassEditScope.SelectedFile
        };

        // A local search counts its groups, as there is only the one file
        Run = Runner.Start(query, sources, Mode is GparamSearchMode.Local, MaxResults);
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
            LOC.Get("GPARAM_SearchTool_Progress_Header"),
            LOC.Get("GPARAM_SearchTool_Progress_Header_TT"));

        var total = Math.Max(run.Total, 1);
        var fraction = Math.Clamp((float)run.Progress / total, 0, 1);

        ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get("GPARAM_SearchTool_Progress", run.Progress, run.Total));

        GUI.WrappedText(LOC.Get("GPARAM_SearchTool_Searching", run.Status));

        GUI.MultiButtonInput($"gparamSearchProgressActions_{Id}",
            $"gparamSearchCancel_{Id}",
            LOC.Get("GPARAM_SearchTool_Cancel"),
            LOC.Get("GPARAM_SearchTool_Cancel_TT"),
            CancelRun);
    }

    private string GetResultLabel(GparamQueryMatch result)
    {
        // A match on the file itself is already labelled by its name
        if (ShowFileInLabel && result.GroupKey != null)
            return $"{result.FileEntry.Filename}  {result.Label}";

        return result.Label;
    }

    private void DisplayResults()
    {
        if (Results == null)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("GPARAM_SearchTool_Results_Header"),
            LOC.Get("GPARAM_SearchTool_Results_Header_TT"));

        var filtered = GetFilteredResults();

        var truncated = Run is { Truncated: true };

        var countText = truncated
            ? LOC.Get("GPARAM_SearchTool_Results_Truncated", MaxResults)
            : LOC.Get("GPARAM_SearchTool_Results", Results.Count);

        if (filtered.Count != Results.Count)
        {
            countText = $"{countText} {LOC.Get("GPARAM_SearchTool_Results_Filtered", filtered.Count)}";
        }

        GUI.WrappedText(countText);

        if (Results.Count == 0)
            return;

        // The results can be edited all at once, by finding them again in the mass edit. A file match has
        // nothing the mass edit acts on.
        if (ResultQuery != null && ResultQuery.Level is not GparamQueryLevel.File)
        {
            GUI.MultiButtonInput($"gparamSearchSendActions_{Id}",
                $"gparamSearchSend_{Id}",
                LOC.Get("GPARAM_SearchTool_Send_To_Mass_Edit"),
                LOC.Get("GPARAM_SearchTool_Send_To_Mass_Edit_TT"),
                () => View.ToolView.MassEditTool.LoadQuery(ResultQuery, ResultScope));
        }

        EditorFilters.DisplayFramedListFilter($"gparamSearchResultFilter_{Id}", ref ResultFilter, ref ExactResultFilter);

        ImGui.BeginChild($"##gparamSearchResults_{Id}", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        GparamQueryMatch jumpTarget = null;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var (result, label) = Results[index];

                if (ImGui.Button($"{Icons.LocationArrow}##gparamSearchJump_{Id}{index}"))
                {
                    jumpTarget = result;
                }
                GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Jump_TT"));

                ImGui.SameLine();

                if (ImGui.Selectable($"{label}##gparamSearchResult_{Id}{index}", IsSelected(result)))
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
    /// Whether the result is what the editor has selected, at the level it was found at
    /// </summary>
    private bool IsSelected(GparamQueryMatch result)
    {
        var selection = View.Selection;

        if (!selection.IsSelectedFile(result.FileEntry))
            return false;

        if (result.GroupKey == null)
            return true;

        if (selection._selectedParamGroupKey != result.GroupKey)
            return false;

        if (result.FieldKey == null)
            return true;

        if (selection._selectedParamFieldKey != result.FieldKey)
            return false;

        return result.ValueIndex < 0 || selection._selectedFieldValueKey == result.ValueID;
    }

    /// <summary>
    /// The positions of the results that match the filter, with only the first of each value when unique values
    /// are asked for. The results arrive while the search runs, so only the new ones are checked, unless the
    /// filter has changed.
    /// </summary>
    private List<int> GetFilteredResults()
    {
        if (FilteredResults == null || FilteredResultsFor != Results ||
            CachedResultFilter != ResultFilter || CachedExactResultFilter != ExactResultFilter ||
            CachedUniqueValues != UniqueValues || CheckedResultCount > Results.Count)
        {
            FilteredResults = new();
            FilteredResultsFor = Results;
            CachedResultFilter = ResultFilter;
            CachedExactResultFilter = ExactResultFilter;
            CachedUniqueValues = UniqueValues;
            SeenValues = new();
            CheckedResultCount = 0;
        }

        for (int i = CheckedResultCount; i < Results.Count; i++)
        {
            var (result, label) = Results[i];

            if (!EditorFilters.IsMatch(ResultFilter, label, ExactResultFilter))
                continue;

            // Only value matches have a value to compare
            if (UniqueValues && result.ValueText != null && !SeenValues.Add(result.ValueText))
                continue;

            FilteredResults.Add(i);
        }

        CheckedResultCount = Results.Count;

        return FilteredResults;
    }
    #endregion
}

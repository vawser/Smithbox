using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Finds value rows, fields or groups, then edits, deletes or duplicates all of them at once. The Find tab narrows
/// down the matches; the Edit tab works out what each of them will become, shows it, and applies it as one undo step.
/// Modelled on the Time Act Editor's mass edit, and runs its finds through the search's <see cref="GparamQueryRunner"/>.
/// </summary>
public partial class GparamMassEditTool
{
    public GparamEditorView View;
    public ProjectEntry Project;

    private readonly GparamQueryRunner Runner;

    /// <summary>
    /// A condition as it is being edited. It is copied into a <see cref="GparamMassEditCondition"/> on Find.
    /// </summary>
    private class ConditionInput
    {
        public GparamQueryValueTarget Target = GparamQueryValueTarget.Value;
        public GparamQueryComparison Comparison = GparamQueryComparison.Equals;
        public string Value = "";
        public string Value2 = "";

        public GparamMassEditCondition ToCondition() => new(Target, Comparison, Value.Trim(), Value2.Trim());
    }

    private GparamMassEditScope Scope = GparamMassEditScope.SelectedFile;
    private GparamMassEditTarget Target = GparamMassEditTarget.Values;

    private string FileText = "";
    private string GroupText = "";
    private string FieldText = "";
    private List<ConditionInput> Conditions = [new()];
    private bool Exact = false;

    // Raised by every change to the inputs, so the matches can tell they no longer show what the inputs ask for
    private int InputsVersion;
    private int FoundVersion = -1;

    // What the scope pointed to when Find was pressed
    private FileDictionaryEntry FoundFile;
    private string FoundGroupKey;
    private string FoundFieldKey;
    private int[] FoundValueKeys;

    private GparamQueryRun Run;

    private record ResultRow(GparamQueryMatch Match, string Label);

    private List<ResultRow> Results;
    private HashSet<(FileDictionaryEntry, string, string)> ResultFields;
    private HashSet<FileDictionaryEntry> ResultFiles;
    private FileDictionaryEntry ResultSource;
    private bool ResultsGlobal;
    private GparamMassEditTarget ResultTarget;

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

    public GparamMassEditTool(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Runner = new GparamQueryRunner(view, project);
        Planner = new GparamMassEditPlanner(project);
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("GPARAM_MassEditTool_Header")}##gparamMassEditToolHeader"))
            return;

        ImGui.BeginChild("GparamMassEditToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("GPARAM_MassEditTool_Hint"));

        var selection = View.Selection;
        var source = selection.IsFileSelected() ? selection.SelectedFileEntry : null;

        // Local matches point into the file they were found in. Matches across all files are kept while the user
        // jumps between them, which is how they are looked at.
        if (Results != null && !ResultsGlobal && ResultSource != source)
        {
            ClearResults();
        }

        // A search handed its query over
        if (PendingFind)
        {
            PendingFind = false;

            if (GetScopeUnavailableReason() == null)
            {
                StartFind(source);
            }
        }

        // Picked up whichever tab is open, so a find keeps going while the Edit tab is shown
        PickUpResults();

        if (ImGui.BeginTabBar("##gparamMassEditTabs"))
        {
            var findFlags = SelectFindTab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;
            SelectFindTab = false;

            // Find
            if (ImGui.BeginTabItem($"{LOC.Get("GPARAM_MassEditTool_Tab_Find")}##gparamMassEditTabFind", findFlags))
            {
                DisplayScope();
                DisplayTarget();
                DisplayFilter();

                if (DisplayActions())
                {
                    StartFind(source);
                }

                DisplayProgress();
                DisplayResults();

                ImGui.EndTabItem();
            }
            GUI.Tooltip(LOC.Get("GPARAM_MassEditTool_Tab_Find_TT"));

            // Edit
            if (ImGui.BeginTabItem($"{LOC.Get("GPARAM_MassEditTool_Tab_Edit")}##gparamMassEditTabEdit"))
            {
                DisplayEdit();

                ImGui.EndTabItem();
            }
            GUI.Tooltip(LOC.Get("GPARAM_MassEditTool_Tab_Edit_TT"));

            ImGui.EndTabBar();
        }
    }

    private void Changed()
    {
        InputsVersion++;
    }

    /// <summary>
    /// Takes over the inputs of a search, so its results can be edited all at once. Its value becomes a condition,
    /// with an exact search's contains turned into equals. The tool is shown, and Find runs once it is next drawn.
    /// </summary>
    public void LoadQuery(GparamQuery query, GparamMassEditScope scope)
    {
        Target = query.Level switch
        {
            GparamQueryLevel.Group => GparamMassEditTarget.Groups,
            GparamQueryLevel.Field => GparamMassEditTarget.Fields,
            _ => GparamMassEditTarget.Values
        };

        // A scope inside what the target finds widens to the nearest one that holds it, before Find runs
        while (!IsScopeAllowed(scope, Target))
        {
            scope++;
        }

        Scope = scope;

        FileText = query.File ?? "";
        GroupText = query.Group ?? "";
        FieldText = query.Field ?? "";
        Exact = query.Exact;

        var condition = new ConditionInput();

        if (query.HasValue)
        {
            condition.Target = query.ValueTarget;
            condition.Comparison = query.Comparison is GparamQueryComparison.Contains
                ? query.Exact ? GparamQueryComparison.Equals : GparamQueryComparison.Contains
                : query.Comparison;
            condition.Value = query.Value ?? "";
            condition.Value2 = query.Value2 ?? "";
        }

        Conditions = [condition];

        Changed();
        ClearResults();

        CFG.Current.Interface_GparamEditor_Tool_MassEdit = true;

        PendingFind = true;
        SelectFindTab = true;
    }

    #region Scope and Target
    private void DisplayScope()
    {
        // Scope
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("GPARAM_MassEditTool_Scope_Header"),
            LOC.Get("GPARAM_MassEditTool_Scope_Header_TT"));

        // A scope inside what the target finds can't hold it: groups can't be found in a field. It widens to the
        // nearest one that can, the scopes running from narrowest to widest.
        if (!IsScopeAllowed(Scope, Target))
        {
            while (!IsScopeAllowed(Scope, Target))
            {
                Scope++;
            }

            Changed();
        }

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##gparamMassEditScope", GetScopeName(Scope)))
        {
            foreach (var scope in Enum.GetValues<GparamMassEditScope>())
            {
                if (!IsScopeAllowed(scope, Target))
                    continue;

                if (ImGui.Selectable($"{GetScopeName(scope)}##gparamMassEditScope{scope}", Scope == scope))
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

    private static bool IsScopeAllowed(GparamMassEditScope scope, GparamMassEditTarget target) => target switch
    {
        GparamMassEditTarget.Groups => scope is GparamMassEditScope.SelectedFile or GparamMassEditScope.AllFiles,
        GparamMassEditTarget.Fields => scope is not (GparamMassEditScope.SelectedValues or GparamMassEditScope.SelectedField),
        _ => true
    };

    /// <summary>
    /// The scope needs the selection to point at what it covers. Gives the reason it can't be used, or null.
    /// </summary>
    private string GetScopeUnavailableReason()
    {
        var selection = View.Selection;

        if (Scope is GparamMassEditScope.AllFiles)
            return null;

        if (!selection.IsFileSelected())
            return LOC.Get("GPARAM_MassEditTool_No_File");

        return Scope switch
        {
            GparamMassEditScope.SelectedGroup or GparamMassEditScope.SelectedField or GparamMassEditScope.SelectedValues
                when !selection.IsGparamGroupSelected() => LOC.Get("GPARAM_MassEditTool_Scope_No_Group"),
            GparamMassEditScope.SelectedField or GparamMassEditScope.SelectedValues
                when !selection.IsGparamFieldSelected() => LOC.Get("GPARAM_MassEditTool_Scope_No_Field"),
            GparamMassEditScope.SelectedValues when !selection.HasValidFieldValueSelection() =>
                LOC.Get("GPARAM_MassEditTool_Scope_No_Values"),
            _ => null
        };
    }

    private static string GetScopeName(GparamMassEditScope scope) => scope switch
    {
        GparamMassEditScope.SelectedValues => LOC.Get("GPARAM_MassEditTool_Scope_Selected_Values"),
        GparamMassEditScope.SelectedField => LOC.Get("GPARAM_MassEditTool_Scope_Selected_Field"),
        GparamMassEditScope.SelectedGroup => LOC.Get("GPARAM_MassEditTool_Scope_Selected_Group"),
        GparamMassEditScope.SelectedFile => LOC.Get("GPARAM_MassEditTool_Scope_Selected_File"),
        GparamMassEditScope.AllFiles => LOC.Get("GPARAM_MassEditTool_Scope_All_Files"),
        _ => $"{scope}"
    };

    private static string GetScopeTooltip(GparamMassEditScope scope) => scope switch
    {
        GparamMassEditScope.SelectedValues => LOC.Get("GPARAM_MassEditTool_Scope_Selected_Values_TT"),
        GparamMassEditScope.SelectedField => LOC.Get("GPARAM_MassEditTool_Scope_Selected_Field_TT"),
        GparamMassEditScope.SelectedGroup => LOC.Get("GPARAM_MassEditTool_Scope_Selected_Group_TT"),
        GparamMassEditScope.SelectedFile => LOC.Get("GPARAM_MassEditTool_Scope_Selected_File_TT"),
        GparamMassEditScope.AllFiles => LOC.Get("GPARAM_MassEditTool_Scope_All_Files_TT"),
        _ => ""
    };

    private void DisplayTarget()
    {
        // Target
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("GPARAM_MassEditTool_Target_Header"),
            LOC.Get("GPARAM_MassEditTool_Target_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##gparamMassEditTarget", GetTargetName(Target)))
        {
            foreach (var target in Enum.GetValues<GparamMassEditTarget>())
            {
                if (ImGui.Selectable($"{GetTargetName(target)}##gparamMassEditTarget{target}", Target == target))
                {
                    if (Target != target)
                    {
                        Target = target;
                        Changed();
                    }
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("GPARAM_MassEditTool_Target_Header_TT"));
    }

    private static string GetTargetName(GparamMassEditTarget target) => target switch
    {
        GparamMassEditTarget.Values => LOC.Get("GPARAM_MassEditTool_Target_Values"),
        GparamMassEditTarget.Fields => LOC.Get("GPARAM_MassEditTool_Target_Fields"),
        GparamMassEditTarget.Groups => LOC.Get("GPARAM_MassEditTool_Target_Groups"),
        _ => $"{target}"
    };
    #endregion

    #region Filter
    private void DisplayFilter()
    {
        // The selected file is the only one a local scope covers
        if (Scope is GparamMassEditScope.AllFiles)
        {
            TextInput("GPARAM_MassEditTool_File", "##gparamMassEditFile", ref FileText);
        }

        // A group or field scope already picks the group or field
        if (Scope is GparamMassEditScope.SelectedFile or GparamMassEditScope.AllFiles)
        {
            TextInput("GPARAM_MassEditTool_Group", "##gparamMassEditGroup", ref GroupText);
        }

        if (Target is not GparamMassEditTarget.Groups &&
            Scope is not (GparamMassEditScope.SelectedField or GparamMassEditScope.SelectedValues))
        {
            TextInput("GPARAM_MassEditTool_Field", "##gparamMassEditField", ref FieldText);
        }

        if (Target is GparamMassEditTarget.Values)
        {
            DisplayConditions();
        }

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("GPARAM_MassEditTool_Options_Header"),
            LOC.Get("GPARAM_MassEditTool_Options_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("GPARAM_SearchTool_Exact")}##gparamMassEditExact", ref Exact))
        {
            Changed();
        }
        GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Exact_TT"));
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
    /// The rows of conditions on the value rows: [part of the row] [comparison] [value], all of which must match
    /// </summary>
    private void DisplayConditions()
    {
        // Conditions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("GPARAM_MassEditTool_Conditions_Header"),
            LOC.Get("GPARAM_MassEditTool_Conditions_Header_TT"));

        ConditionInput removed = null;

        var spacing = ImGui.GetStyle().ItemSpacing.X;

        for (int i = 0; i < Conditions.Count; i++)
        {
            var condition = Conditions[i];

            if (i > 0)
            {
                ImGui.Separator();
            }

            var removeWidth = DPI.IconButtonSize.X + spacing;
            var comboWidth = (ImGui.GetContentRegionAvail().X - removeWidth - spacing) / 2;

            // Part of the row
            ImGui.SetNextItemWidth(comboWidth);

            if (ImGui.BeginCombo($"##gparamMassEditConditionTarget{i}", GparamQueryRunner.GetValueTargetName(condition.Target)))
            {
                foreach (var target in Enum.GetValues<GparamQueryValueTarget>())
                {
                    if (ImGui.Selectable($"{GparamQueryRunner.GetValueTargetName(target)}##gparamMassEditConditionTarget{i}_{target}",
                        condition.Target == target))
                    {
                        condition.Target = target;
                        Changed();
                    }
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Value_Target_TT"));

            ImGui.SameLine();

            // Comparison
            ImGui.SetNextItemWidth(comboWidth);

            if (ImGui.BeginCombo($"##gparamMassEditConditionComparison{i}", GparamSearchTool.GetComparisonName(condition.Comparison)))
            {
                foreach (var comparison in Enum.GetValues<GparamQueryComparison>())
                {
                    if (ImGui.Selectable($"{GparamSearchTool.GetComparisonName(comparison)}##gparamMassEditConditionComparison{i}_{comparison}",
                        condition.Comparison == comparison))
                    {
                        condition.Comparison = comparison;
                        Changed();
                    }
                    GUI.Tooltip(GparamSearchTool.GetComparisonTooltip(comparison));
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(GparamSearchTool.GetComparisonTooltip(condition.Comparison));

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Minus}##gparamMassEditConditionRemove{i}", DPI.IconButtonSize))
            {
                removed = condition;
            }
            GUI.Tooltip(LOC.Get("GPARAM_MassEditTool_Condition_Remove_TT"));

            // Value
            if (condition.Comparison is GparamQueryComparison.Between)
            {
                var width = (ImGui.GetContentRegionAvail().X - spacing) / 2;

                ImGui.SetNextItemWidth(width);

                if (ImGui.InputTextWithHint($"##gparamMassEditConditionValue{i}", LOC.Get("GPARAM_SearchTool_Value_Low_Hint"),
                    ref condition.Value, 255))
                {
                    Changed();
                }
                GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Value_Between_TT"));

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);

                if (ImGui.InputTextWithHint($"##gparamMassEditConditionValue2{i}", LOC.Get("GPARAM_SearchTool_Value_High_Hint"),
                    ref condition.Value2, 255))
                {
                    Changed();
                }
                GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Value_Between_TT"));
            }
            else
            {
                ImGui.SetNextItemWidth(-1);

                if (ImGui.InputTextWithHint($"##gparamMassEditConditionValue{i}", LOC.Get("GPARAM_MassEditTool_Condition_Value_Hint"),
                    ref condition.Value, 255))
                {
                    Changed();
                }
                GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Value_TT"));
            }
        }

        if (removed != null)
        {
            Conditions.Remove(removed);
            Changed();
        }

        GUI.MultiButtonInput("gparamMassEditConditionActions",
            "gparamMassEditConditionAdd",
            LOC.Get("GPARAM_MassEditTool_Condition_Add"),
            LOC.Get("GPARAM_MassEditTool_Condition_Add_TT"),
            () =>
            {
                Conditions.Add(new());
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
            LOC.Get("GPARAM_MassEditTool_Actions_Header"),
            LOC.Get("GPARAM_MassEditTool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("gparamMassEditActions",
            "gparamMassEditFind",
            LOC.Get("GPARAM_MassEditTool_Find"),
            LOC.Get("GPARAM_MassEditTool_Find_TT"),
            () => find = true,
            canFind,

            "gparamMassEditClear",
            LOC.Get("GPARAM_MassEditTool_Clear"),
            LOC.Get("GPARAM_MassEditTool_Clear_TT"),
            ClearInputs,
            true);

        return find && canFind;
    }

    private void ClearInputs()
    {
        FileText = "";
        GroupText = "";
        FieldText = "";
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
        ResultFields = null;
        ResultFiles = null;
        ResultSource = null;
        ResultsGlobal = false;
        FoundVersion = -1;

        ClearPlan();
    }

    /// <summary>
    /// Takes what the scope covers from the selection on the main thread, then hands the filter to a background task
    /// </summary>
    private void StartFind(FileDictionaryEntry source)
    {
        ClearResults();

        var selection = View.Selection;
        var global = Scope is GparamMassEditScope.AllFiles;

        string groupKey = Scope is GparamMassEditScope.SelectedGroup or GparamMassEditScope.SelectedField or GparamMassEditScope.SelectedValues
            ? selection._selectedParamGroupKey
            : null;

        string fieldKey = Scope is GparamMassEditScope.SelectedField or GparamMassEditScope.SelectedValues
            ? selection._selectedParamFieldKey
            : null;

        var sources = global
            ? Runner.GetGlobalSources()
            : [Runner.GetLocalSource(source, selection.GetSelectedGparam(), groupKey, fieldKey)];

        HashSet<GPARAM.IFieldValue> limitValues = Scope is GparamMassEditScope.SelectedValues
            ? selection.GetSelectedValues().ToHashSet()
            : null;

        // The inputs a scope already picks for are left out, as they are hidden
        var filter = new GparamMassEditFilter
        {
            Target = Target,
            File = global ? FileText.Trim() : "",
            Group = groupKey == null ? GroupText.Trim() : "",
            Field = fieldKey == null && Target is not GparamMassEditTarget.Groups ? FieldText.Trim() : "",
            Conditions = Target is GparamMassEditTarget.Values ? Conditions.Select(e => e.ToCondition()).ToList() : [],
            Exact = Exact,
            LimitValues = limitValues
        };

        Results = new();
        ResultFields = new();
        ResultFiles = new();
        ResultSource = source;
        ResultsGlobal = global;
        ResultTarget = Target;

        FoundVersion = InputsVersion;
        FoundFile = selection.SelectedFileEntry;
        FoundGroupKey = selection._selectedParamGroupKey;
        FoundFieldKey = selection._selectedParamFieldKey;
        FoundValueKeys = selection.GetSelectedValues().Select(e => e.ID).ToArray();

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

        var selection = View.Selection;

        return Scope switch
        {
            GparamMassEditScope.SelectedValues =>
                selection._selectedParamFieldKey != FoundFieldKey || selection._selectedParamGroupKey != FoundGroupKey ||
                !selection.GetSelectedValues().Select(e => e.ID).SequenceEqual(FoundValueKeys),
            GparamMassEditScope.SelectedField =>
                selection._selectedParamFieldKey != FoundFieldKey || selection._selectedParamGroupKey != FoundGroupKey,
            GparamMassEditScope.SelectedGroup => selection._selectedParamGroupKey != FoundGroupKey,
            GparamMassEditScope.SelectedFile => !selection.IsSelectedFile(FoundFile),
            _ => false
        };
    }
    #endregion

    #region Results
    /// <summary>
    /// Puts the file in front of a label when the matches span files
    /// </summary>
    private string PrefixLabel(GparamQueryMatch match, string label)
    {
        return ResultsGlobal ? $"{match.FileEntry.Filename}  {label}" : label;
    }

    /// <summary>
    /// How many matches there are, and in how many fields and files
    /// </summary>
    private string GetMatchCountText()
    {
        var text = ResultTarget switch
        {
            GparamMassEditTarget.Values => LOC.Get("GPARAM_MassEditTool_Matches_Values", Results.Count, ResultFields.Count),
            GparamMassEditTarget.Fields => LOC.Get("GPARAM_MassEditTool_Matches_Fields", Results.Count),
            _ => LOC.Get("GPARAM_MassEditTool_Matches_Groups", Results.Count)
        };

        if (ResultsGlobal)
        {
            text = $"{text} {LOC.Get("GPARAM_MassEditTool_Matches_Files", ResultFiles.Count)}";
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
            ResultFields.Add((match.FileEntry, match.GroupKey, match.FieldKey));
            ResultFiles.Add(match.FileEntry);
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
            LOC.Get("GPARAM_SearchTool_Progress_Header"),
            LOC.Get("GPARAM_SearchTool_Progress_Header_TT"));

        var total = Math.Max(run.Total, 1);
        var fraction = Math.Clamp((float)run.Progress / total, 0, 1);

        ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get("GPARAM_SearchTool_Progress", run.Progress, run.Total));

        GUI.WrappedText(LOC.Get("GPARAM_SearchTool_Searching", run.Status));

        GUI.MultiButtonInput("gparamMassEditProgressActions",
            "gparamMassEditCancel",
            LOC.Get("GPARAM_SearchTool_Cancel"),
            LOC.Get("GPARAM_SearchTool_Cancel_TT"),
            CancelRun);
    }

    private void DisplayResults()
    {
        if (Results == null)
            return;

        // Matches
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("GPARAM_MassEditTool_Matches_Header"),
            LOC.Get("GPARAM_MassEditTool_Matches_Header_TT"),
            ref ShowMatchesList);

        var countText = GetMatchCountText();

        var filtered = GetFilteredResults();

        if (filtered.Count != Results.Count)
        {
            countText = $"{countText} {LOC.Get("GPARAM_SearchTool_Results_Filtered", filtered.Count)}";
        }

        GUI.WrappedText(countText);

        if (IsStale())
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("GPARAM_MassEditTool_Matches_Stale"));
        }

        // The counts stay on show when the list is hidden
        if (Results.Count == 0 || !ShowMatchesList)
            return;

        EditorFilters.DisplayFramedListFilter("gparamMassEditResultFilter", ref ResultFilter, ref ExactResultFilter);

        // The list is the last thing in its tab, so it takes the rest of the height
        ImGui.BeginChild("##gparamMassEditResults", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        GparamQueryMatch jumpTarget = null;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var (match, label) = Results[index];

                if (ImGui.Button($"{Icons.LocationArrow}##gparamMassEditJump{index}"))
                {
                    jumpTarget = match;
                }
                GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Jump_TT"));

                ImGui.SameLine();

                if (ImGui.Selectable($"{label}##gparamMassEditResult{index}", IsSelected(match)))
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

    /// <summary>
    /// Whether the match is what the editor has selected, at the level it was found at
    /// </summary>
    private bool IsSelected(GparamQueryMatch match)
    {
        var selection = View.Selection;

        if (!selection.IsSelectedFile(match.FileEntry) || selection._selectedParamGroupKey != match.GroupKey)
            return false;

        if (match.FieldKey == null)
            return true;

        if (selection._selectedParamFieldKey != match.FieldKey)
            return false;

        return match.ValueIndex < 0 || selection._selectedFieldValueKey == match.ValueID;
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

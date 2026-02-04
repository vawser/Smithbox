using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class FieldValueFinder
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public string imguiID = "FieldValueFinder";

    public string SearchText = "";
    public string CachedSearchText = "";
    public string RangeSearchText_Start = "";
    public string RangeSearchText_End = "";

    public bool UseRangeMatchMode = false;
    public bool DisplayFirstMatchOnlyInResult = false;
    public bool DisplayCommunityNameInResult = false;

    public List<string> TargetedParams = new();

    public List<DataSearchResult> Results = new();

    public FieldValueFinder(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        if (Editor.Project.Handler.ParamData.PrimaryBank.Params == null)
        {
            return;
        }

        var windowWidth = ImGui.GetWindowWidth();

        var Size = ImGui.GetWindowSize();
        float EditX = (Size.X / 100) * 95;
        float EditY = (Size.Y / 100) * 25;

        UIHelper.WrappedText("Display all instances of a specified field value.");
        UIHelper.WrappedText("");

        ImGui.Checkbox("Display Row Context Action", ref CFG.Current.ParamEditor_Row_Context_Display_Finder_Quick_Option);
        UIHelper.Tooltip("If enabled, a quick search option will appear in the right-click Row Context menu within the Row List.");

        /// Targeted Param
        UIHelper.SimpleHeader("Targeted Params", "Leave blank to target all params.");

        // Add
        if (ImGui.Button($"{Icons.Plus}##paramTargetAdd_fieldValueFinder"))
        {
            TargetedParams.Add("");
        }
        UIHelper.Tooltip("Add new param target input row.");

        ImGui.SameLine();

        // Remove
        if (TargetedParams.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_fieldValueFinder"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
            }
            UIHelper.Tooltip("Remove last added param target input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_fieldValueFinder"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
                UIHelper.Tooltip("Remove last added param target input row.");
            }
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##paramTargetReset_fieldValueFinder"))
        {
            TargetedParams = new List<string>();
        }
        UIHelper.Tooltip("Reset param target input rows.");

        for (int i = 0; i < TargetedParams.Count; i++)
        {
            var curCommand = TargetedParams[i];
            var curText = curCommand;

            if (ImGui.InputText($"##paramTargetInput{i}_fieldValueFinder", ref curText, 255))
            {
                TargetedParams[i] = curText;
            }
            UIHelper.Tooltip("The param target to include.");
        }

        UIHelper.WrappedText("");

        /// Search Configuration
        UIHelper.SimpleHeader("Search Configuration", "The configuration parameters for the search.");

        // Checkbox: Enable Range Search
        ImGui.Checkbox($"Enable Range Search##rangeMode_{imguiID}", ref UseRangeMatchMode);

        UIHelper.Tooltip("If enabled, the search will search for matches between a start and end value.");

        // Checkbox: Display First Match Only
        ImGui.Checkbox($"Display First Match Only##firstMatchOnly_{imguiID}", ref DisplayFirstMatchOnlyInResult);

        UIHelper.Tooltip("Only display the first match within a param, instead of all matches.");

        // Checkbox: Display Community Name in Result
        ImGui.Checkbox($"Display Community Names in Result##displayCommunityNames_{imguiID}",
            ref DisplayCommunityNameInResult);
        UIHelper.Tooltip("Display the community name for the field instead of the internal name.");

        UIHelper.WrappedText("");

        if (UseRangeMatchMode)
        {
            // Start Value
            UIHelper.WrappedText("Start Value:");
            ImGui.InputText($"##startSearchValue_{imguiID}", ref RangeSearchText_Start, 255);
            UIHelper.Tooltip("The start value in the search range.");

            // End Value
            UIHelper.WrappedText("End Value:");
            ImGui.InputText($"##endSearchValue_{imguiID}", ref RangeSearchText_End, 255);
            UIHelper.Tooltip("The end value in the search range.");
        }

        if (!UseRangeMatchMode)
        {
            UIHelper.WrappedText("Search Value:");
            ImGui.InputText($"##searchValue_{imguiID}", ref SearchText, 255);
            UIHelper.Tooltip("The value to search for.");
        }

        if (ImGui.Button($"Search##searchButton_{imguiID}"))
        {
            CachedSearchText = SearchText;

            if (UseRangeMatchMode)
            {
                CachedSearchText = $"{RangeSearchText_Start} -> {RangeSearchText_End}";
            }

            Results = ConstructResults();
            Results.Sort();
        }

        // Result List
        if (Results.Count > 0)
        {
            UIHelper.SimpleHeader("Search Results", "The results of the last search performed.");

            UIHelper.WrappedText($"Search Term:");
            UIHelper.DisplayAlias(CachedSearchText);

            UIHelper.WrappedText($"Result Count:");
            UIHelper.DisplayAlias($"{Results.Count}");

            UIHelper.WrappedText($"");
            UIHelper.WrappedText($"Param: Row ID: Field Name: Field Value");

            ImGui.BeginChild($"##resultSection_{imguiID}",
                new Vector2(EditX, EditY));

            foreach (var result in Results)
            {
                var name = result.FieldInternalName;

                if (DisplayCommunityNameInResult)
                {
                    name = result.FieldDisplayName;
                }

                if (ImGui.Selectable($"{result.ParamName}: {result.RowID} {result.RowName}: {name}: {result.FieldValue}##resultEntry_{imguiID}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result.ParamName}/{result.RowID}/{result.FieldInternalName}");
                }
                if (ImGui.BeginPopupContextItem($"#resultPopup{result.ParamName}{result.RowID}"))
                {
                    if (ImGui.Selectable("Copy Row ID"))
                    {
                        PlatformUtils.Instance.SetClipboardText($"{result.RowID}");
                    }

                    if (ImGui.Selectable("Copy Row Name"))
                    {
                        if (result.RowName != null)
                        {
                            PlatformUtils.Instance.SetClipboardText(result.RowName);
                        }
                    }

                    ImGui.EndPopup();
                }
            }
            ImGui.EndChild();
        }
        else
        {
            ImGui.Text("No results to display.");
        }

        UIHelper.WrappedText("");

    }

    /// <summary>
    /// Construct the results list when the search button is used.
    /// </summary>
    public List<DataSearchResult> ConstructResults()
    {
        List<DataSearchResult> output = new();

        var searchValue = SearchText;
        var startValue = RangeSearchText_Start;
        var endValue = RangeSearchText_End;

        foreach (var p in Editor.Project.Handler.ParamData.PrimaryBank.Params)
        {
            if (TargetedParams.Count > 0)
            {
                if (!TargetedParams.Contains(p.Key))
                {
                    continue;
                }
            }

            var meta = Editor.Project.Handler.ParamData.GetParamMeta(p.Value.AppliedParamdef);

            for (var i = 0; i < p.Value.Rows.Count; i++)
            {
                var r = p.Value.Rows[i];
                var id = r.ID;

                string fieldName = "";
                string fieldDisplayName = "";

                foreach (var field in r.Cells)
                {
                    PARAMDEF.DefType type = field.Def.DisplayType;

                    var fieldMeta = Editor.Project.Handler.ParamData.GetParamFieldMeta(meta, field.Def);

                    fieldName = field.Def.InternalName;
                    if (fieldMeta != null)
                    {
                        fieldDisplayName = fieldMeta.AltName;
                    }

                    var isMatch = false;
                    var fieldValue = "";
                    (isMatch, fieldValue) = DataFinderUtil.IsValueMatch(type, field, searchValue, UseRangeMatchMode, startValue, endValue);

                    if (isMatch)
                    {
                        var result = new DataSearchResult();
                        result.RowID = id;
                        result.RowName = r.Name;
                        result.ParamName = p.Key;
                        result.FieldInternalName = fieldName;
                        result.FieldDisplayName = fieldDisplayName;
                        result.FieldValue = fieldValue;

                        output.Add(result);

                        // Skip matching more if this is enabled
                        if (DisplayFirstMatchOnlyInResult)
                        {
                            break;
                        }
                    }
                }
            }
        }

        return output;
    }
}


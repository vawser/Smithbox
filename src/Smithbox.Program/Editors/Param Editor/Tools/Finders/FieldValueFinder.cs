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

        // Header
        UIHelper.WrappedText(LOC.Get("PARAM_FieldValueFinder_Hint"));

        // Options
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("PARAM_DataFinder_Header_Options"),
            LOC.Get("PARAM_DataFinder_Header_Options_TT"));

        // Toggle: Enable Range Search
        ImGui.Checkbox($"{LOC.Get("PARAM_FieldValueFinder_Checkbox_Enable_Range_Search")}##rangeMode_{imguiID}", 
            ref UseRangeMatchMode);
        UIHelper.Tooltip(LOC.Get("PARAM_FieldValueFinder_Checkbox_Enable_Range_Search_TT"));

        // Toggle: Display First Match Only
        ImGui.Checkbox($"{LOC.Get("PARAM_FieldValueFinder_Checkbox_Display_First_Match")}##firstMatchOnly_{imguiID}", 
            ref DisplayFirstMatchOnlyInResult);
        UIHelper.Tooltip(LOC.Get("PARAM_FieldValueFinder_Checkbox_Display_First_Match_TT"));

        // Toggle: Display Community Name in Result
        ImGui.Checkbox($"{LOC.Get("PARAM_FieldValueFinder_Checkbox_Display_Community_Name")}##displayCommunityNames_{imguiID}",
            ref DisplayCommunityNameInResult);
        UIHelper.Tooltip(LOC.Get("PARAM_FieldValueFinder_Checkbox_Display_Community_Name_TT"));

        // Targeted Param
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("PARAM_DataFinder_Header_Target_Params"),
            LOC.Get("PARAM_DataFinder_Header_Target_Params_TT"));

        // Add
        if (ImGui.Button($"{Icons.Plus}##paramTargetAdd_{imguiID}"))
        {
            TargetedParams.Add("");
        }
        UIHelper.Tooltip(LOC.Get("PARAM_DataFinder_Action_Add_Param_Target_TT"));

        ImGui.SameLine();

        // Remove
        if (TargetedParams.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_{imguiID}"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
            }
            UIHelper.Tooltip(LOC.Get("PARAM_DataFinder_Action_Remove_Param_Target_TT"));

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_{imguiID}"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
            }
            UIHelper.Tooltip(LOC.Get("PARAM_DataFinder_Action_Remove_Param_Target_TT"));
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button($"{LOC.Get("PARAM_DataFinder_Action_Reset_Param_Target")}##paramTargetReset_{imguiID}"))
        {
            TargetedParams = new List<string>();
        }
        UIHelper.Tooltip(LOC.Get("PARAM_DataFinder_Action_Reset_Param_Target_TT"));

        for (int i = 0; i < TargetedParams.Count; i++)
        {
            var curCommand = TargetedParams[i];
            var curText = curCommand;

            ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.5f);
            if (ImGui.InputText($"##paramTargetInput{i}_fieldValueFinder", ref curText, 255))
            {
                TargetedParams[i] = curText;
            }
            UIHelper.Tooltip(LOC.Get("PARAM_DataFinder_Param_Target_Include_TT"));
        }

        UIHelper.Spacer();

        // Search Text
        UIHelper.SimpleHeader(
            LOC.Get("PARAM_DataFinder_Header_Search"),
            LOC.Get("PARAM_DataFinder_Header_Search_TT"));

        if (UseRangeMatchMode)
        {
            UIHelper.SetInputWidth();
            ImGui.InputTextWithHint($"{LOC.Get("PARAM_FieldValueFinder_Value_Start")}##startSearchValue_{imguiID}", LOC.Get("PARAM_DataFinder_Search_RangeStart_Hint"), ref RangeSearchText_Start, 255);

            UIHelper.SetInputWidth();
            ImGui.InputTextWithHint($"{LOC.Get("PARAM_FieldValueFinder_Value_End")}##endSearchValue_{imguiID}", LOC.Get("PARAM_DataFinder_Search_RangeEnd_Hint"), ref RangeSearchText_End, 255);
        }

        if (!UseRangeMatchMode)
        {
            UIHelper.SetInputWidth();
            ImGui.InputTextWithHint($"{LOC.Get("PARAM_FieldValueFinder_Value")}##searchValue_{imguiID}", LOC.Get("PARAM_DataFinder_Search_Value_Hint"), ref SearchText, 255);
        }

        UIHelper.MultiButtonInput("searchActions",
            "search",
            LOC.Get("PARAM_DataFinder_Action_Search"),
            LOC.Get("PARAM_DataFinder_Action_Search_TT"),
            ConductSearch,

            "clearSearch",
            LOC.Get("PARAM_DataFinder_Action_Clear"),
            LOC.Get("PARAM_DataFinder_Action_Clear_TT"),
            ClearSearch);

        UIHelper.Spacer();

        // Result List
        if (Results.Count > 0)
        {
            UIHelper.SimpleHeader(
                LOC.Get("PARAM_DataFinder_Header_Search_Results"),
                LOC.Get("PARAM_DataFinder_Header_Search_Results_TT"));

            UIHelper.WrappedText(LOC.Get("PARAM_DataFinder_Search_Term"));
            UIHelper.DisplayAlias(CachedSearchText);

            UIHelper.WrappedText(LOC.Get("PARAM_DataFinder_Result_Count"));
            UIHelper.DisplayAlias($"{Results.Count}");

            UIHelper.Spacer();
            UIHelper.WrappedText(LOC.Get("PARAM_FieldValueFinder_Results_Column_Header"));

            ImGui.BeginChild($"##resultSection_{imguiID}",
                new Vector2(0, ImGui.GetContentRegionAvail().Y * 0.9f), ImGuiChildFlags.Borders);

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
                if (ImGui.BeginPopupContextItem($"##resultPopup{result.ParamName}{result.RowID}"))
                {
                    if (ImGui.Selectable($"{LOC.Get("PARAM_FieldValueFinder_Context_Action_Copy_Row_ID")}##copyRowIdAction"))
                    {
                        PlatformUtils.Instance.SetClipboardText($"{result.RowID}");
                    }

                    if (ImGui.Selectable($"{LOC.Get("PARAM_FieldValueFinder_Context_Action_Copy_Row_Name")}##copyRowNameAction"))
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
            ImGui.Text(LOC.Get("PARAM_DataFinder_No_Results"));
        }

        UIHelper.WrappedText("");

    }

    public void ConductSearch()
    {
        CachedSearchText = SearchText;

        if (UseRangeMatchMode)
        {
            CachedSearchText = $"{RangeSearchText_Start} -> {RangeSearchText_End}";
        }

        Results = ConstructResults();
        Results.Sort();
    }

    public void ClearSearch()
    {
        SearchText = "";
        CachedSearchText = "";
        Results = new();
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
            var annotations = Editor.Project.Handler.ParamData.GetParamAnnotations(p.Value.AppliedParamdef.ParamType);

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
                    var fieldAnnotation = Editor.Project.Handler.ParamData.GetFieldAnnotation(annotations, field.Def.InternalName);

                    fieldName = field.Def.InternalName;
                    if (fieldMeta != null && fieldAnnotation != null)
                    {
                        fieldDisplayName = fieldAnnotation.Name;
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


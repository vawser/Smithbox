using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class FieldNameFinder
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public string imguiID = "FieldNameFinder";

    public string SearchText = "";
    public string CachedSearchText = "";
    public bool IncludeCommunityNameInSearch = true;
    public bool IncludeDescriptionInSearch = true;
    public bool MatchTextExactly = false;
    public bool DisplayCommunityNameInResult = false;

    public List<string> TargetedParams = new();

    public List<DataSearchResult> Results = new();

    public FieldNameFinder(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        if (View.Editor.Project.Handler.ParamData.PrimaryBank.Params == null)
        {
            return;
        }

        var windowWidth = ImGui.GetWindowWidth();

        // Header
        UIHelper.WrappedText(LOC.Get("PARAM_FieldNameFinder_Hint"));

        // Options
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("PARAM_DataFinder_Header_Options"),
            LOC.Get("PARAM_DataFinder_Header_Options_TT"));

        // Toggle: Include Community Name in Search
        ImGui.Checkbox($"{LOC.Get("PARAM_FieldNameFinder_Checkbox_Include_Community_Name")}##includeCommunityName_{imguiID}",
            ref IncludeCommunityNameInSearch);
        UIHelper.Tooltip(LOC.Get("PARAM_FieldNameFinder_Checkbox_Include_Community_Name_TT"));

        // Toggle: Include Descriptions in Search
        ImGui.Checkbox($"{LOC.Get("PARAM_FieldNameFinder_Checkbox_Include_Description")}##includeDescriptions_{imguiID}",
            ref IncludeDescriptionInSearch);
        UIHelper.Tooltip(LOC.Get("PARAM_FieldNameFinder_Checkbox_Include_Description_TT"));

        // Toggle: Match Exactly
        ImGui.Checkbox($"{LOC.Get("PARAM_FieldNameFinder_Checkbox_Complete_Word_Match")}##matchExact_{imguiID}",
            ref MatchTextExactly);
        UIHelper.Tooltip(LOC.Get("PARAM_FieldNameFinder_Checkbox_Complete_Word_Match_TT"));

        // Toggle: Display Community Name in Results
        ImGui.Checkbox($"{LOC.Get("PARAM_FieldNameFinder_Checkbox_Display_Community_Name")}##useCommunityNames_{imguiID}",
            ref DisplayCommunityNameInResult);
        UIHelper.Tooltip(LOC.Get("PARAM_FieldNameFinder_Checkbox_Display_Community_Name_TT"));

        // Targeted Params
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

        // Param Target Entries
        for (int i = 0; i < TargetedParams.Count; i++)
        {
            var curCommand = TargetedParams[i];
            var curText = curCommand;

            ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.5f);
            if (ImGui.InputText($"##paramTargetInput{i}_{imguiID}", ref curText, 255))
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

        UIHelper.SetInputWidth();
        ImGui.InputTextWithHint($"{LOC.Get("PARAM_FieldNameFinder_Name")}##searchInput_{imguiID}", LOC.Get("PARAM_DataFinder_Search_Hint"), ref SearchText, 255);

        // Actions
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

        // Results List
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
            UIHelper.WrappedText(LOC.Get("PARAM_FieldNameFinder_Results_Column_Header"));

            ImGui.BeginChild($"##resultSection_{imguiID}",
                new Vector2(0, ImGui.GetContentRegionAvail().Y * 0.9f), ImGuiChildFlags.Borders);

            foreach (var result in Results)
            {
                var name = result.FieldInternalName;

                if (DisplayCommunityNameInResult)
                {
                    name = result.FieldDisplayName;
                }

                if (ImGui.Selectable($"{result.ParamName}: {name}##resultEntry_{imguiID}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result.ParamName}");
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
        if (View.Editor.Project.Handler.ParamData.PrimaryBank.Params != null)
        {
            CachedSearchText = SearchText;

            Results = ConstructResults();
            Results.Sort();
        }
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
    private List<DataSearchResult> ConstructResults()
    {
        List<DataSearchResult> output = new();

        var searchComponents = SearchText.ToLower().Split(" ");

        foreach (var p in View.Editor.Project.Handler.ParamData.PrimaryBank.Params)
        {
            if (TargetedParams.Count > 0)
            {
                if (!TargetedParams.Contains(p.Key))
                {
                    continue;
                }
            }

            var def = p.Value.AppliedParamdef;
            var meta = View.Editor.Project.Handler.ParamData.GetParamMeta(def);
            var annotations = View.Editor.Project.Handler.ParamData.GetParamAnnotations(def.ParamType);

            foreach (var field in def.Fields)
            {
                bool addResult = false;
                var fieldMeta = View.Editor.Project.Handler.ParamData.GetParamFieldMeta(meta, field);
                var fieldAnnotation = View.Editor.Project.Handler.ParamData.GetFieldAnnotation(annotations, field.InternalName);

                foreach (var entry in searchComponents)
                {
                    // Internal Name
                    if (field.InternalName != null)
                    {
                        if (MatchTextExactly)
                        {
                            var adjustedName = field.InternalName;

                            // Place _ in front of inter-word capitalization
                            adjustedName = Regex.Replace(adjustedName, "(?<!^)([A-Z])", "_$1");

                            var adjustedComponents = adjustedName.Split("_");

                            foreach (var component in adjustedComponents)
                            {
                                if (component.ToLower() == entry)
                                {
                                    addResult = true;
                                }
                            }
                        }
                        else
                        {
                            if (field.InternalName.ToLower().Contains(entry))
                            {
                                addResult = true;
                            }
                        }
                    }

                    // Display Name
                    if (fieldAnnotation != null && fieldAnnotation.Name != null && IncludeCommunityNameInSearch)
                    {
                        var displayNameComponents = fieldAnnotation.Name.Split(" ");

                        foreach (var displayComponent in displayNameComponents)
                        {
                            if (MatchTextExactly)
                            {
                                if (displayComponent.ToLower() == entry)
                                {
                                    addResult = true;
                                }
                            }
                            else
                            {
                                if (displayComponent.ToLower().Contains(entry))
                                {
                                    addResult = true;
                                }
                            }
                        }
                    }

                    // Wiki
                    if (fieldAnnotation != null && fieldAnnotation.Description != null && IncludeDescriptionInSearch)
                    {
                        var descriptionComponents = fieldAnnotation.Description.Split(" ");

                        foreach (var descriptionComponent in descriptionComponents)
                        {
                            if (MatchTextExactly)
                            {
                                if (descriptionComponent.ToLower() == entry)
                                {
                                    addResult = true;
                                }
                            }
                            else
                            {
                                if (descriptionComponent.ToLower().Contains(entry))
                                {
                                    addResult = true;
                                }
                            }
                        }
                    }
                }

                if (addResult)
                {
                    var result = new DataSearchResult();
                    result.FieldInternalName = field.InternalName;

                    if (fieldAnnotation == null)
                    {
                        result.FieldDisplayName = p.Key;
                    }
                    else
                    {
                        result.FieldDisplayName = fieldAnnotation.Name;
                    }

                    result.ParamName = p.Key;

                    output.Add(result);
                }
            }
        }

        return output;
    }
}

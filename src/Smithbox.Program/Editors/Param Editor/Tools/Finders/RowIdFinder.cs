using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

public class RowIdFinder
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public string imguiID = "RowIDFinder";

    public int SearchID = -1;
    public int SearchIndex = -1;
    public int CachedSearchID = -1;
    public bool IncludeDescriptionInSearch = true;
    public bool DisplayCommunityNameInResult = false;

    public List<string> TargetedParams = new();

    public List<DataSearchResult> Results = new();

    public RowIdFinder(ParamEditorView view, ProjectEntry project)
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

        UIHelper.WrappedText(LOC.Get("PARAM_RowIdFinder_Hint"));

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
            if (ImGui.InputText($"##paramTargetInput{i}_rowIdFinder", ref curText, 255))
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

        // Row Index
        UIHelper.SetInputWidth();
        UIHelper.IntInput($"rowIndex_{imguiID}", ref SearchIndex, LOC.Get("PARAM_RowIdFinder_Input_Row_Index"));
        UIHelper.Tooltip(LOC.Get("PARAM_RowIdFinder_Input_Row_Index_TT"));

        // Search Text
        UIHelper.SetInputWidth();
        UIHelper.IntInput($"searchId_{imguiID}", ref SearchID, LOC.Get("PARAM_RowIdFinder_Input_Row_ID"));
        UIHelper.Tooltip(LOC.Get("PARAM_RowIdFinder_Input_Row_ID_TT"));

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
            UIHelper.DisplayAlias($"{CachedSearchID}");

            UIHelper.WrappedText(LOC.Get("PARAM_DataFinder_Result_Count"));
            UIHelper.DisplayAlias($"{Results.Count}");

            UIHelper.Spacer();
            UIHelper.WrappedText(LOC.Get("PARAM_RowIdFinder_Results_Column_Header"));

            ImGui.BeginChild($"##resultSection_{imguiID}",
                new Vector2(0, ImGui.GetContentRegionAvail().Y * 0.9f), ImGuiChildFlags.Borders);

            foreach (var result in Results)
            {
                if (ImGui.Selectable($"{result.ParamName}##resultEntry_{imguiID}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result.ParamName}/{result.RowID}");
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
        CachedSearchID = SearchID;

        Results = ConstructResults();
        Results.Sort();
    }

    public void ClearSearch()
    {
        SearchID = -1;
        CachedSearchID = -1;
        Results = new();
    }

    /// <summary>
    /// Construct the results list when the search button is used.
    /// </summary>
    private List<DataSearchResult> ConstructResults()
    {
        List<DataSearchResult> output = new();

        foreach (var p in View.Editor.Project.Handler.ParamData.PrimaryBank.Params)
        {
            if (TargetedParams.Count > 0)
            {
                if (!TargetedParams.Contains(p.Key))
                {
                    continue;
                }
            }

            for (var i = 0; i < p.Value.Rows.Count; i++)
            {
                var r = p.Value.Rows[i];
                var id = r.ID;

                if (r.ID == SearchID && (SearchIndex == -1 || SearchIndex == i))
                {
                    var result = new DataSearchResult();
                    result.ParamName = p.Key;
                    result.RowID = r.ID;

                    output.Add(result);

                    break;
                }
            }
        }

        return output;
    }
}

using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class IdSetFinder
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public string imguiID = "IdSetFinder";

    public string SearchText = "";
    public string CachedSearchText = "";

    public List<string> Results = new();

    public IdSetFinder(ParamEditorView view, ProjectEntry project)
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

        UIHelper.WrappedText(LOC.Get("PARAM_IdSetFinder_Hint"));

        // Search Text
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("PARAM_DataFinder_Header_Search"),
            LOC.Get("PARAM_DataFinder_Header_Search_TT"));

        UIHelper.SetInputWidth();
        ImGui.InputTextWithHint($"{LOC.Get("PARAM_IdSetFinder_ID_Set")}##searchValue_{imguiID}", LOC.Get("PARAM_DataFinder_Search_Hint"), ref SearchText, 255);
        UIHelper.Tooltip(LOC.Get("PARAM_IdSetFinder_Search_TT"));

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
            UIHelper.WrappedText(LOC.Get("PARAM_IdSetFinder_Results_Column_Header"));

            ImGui.BeginChild($"##resultSection_{imguiID}",
               new Vector2(0, ImGui.GetContentRegionAvail().Y * 0.9f), ImGuiChildFlags.Borders);

            foreach (var result in Results)
            {
                if (ImGui.Selectable($"{result}##resultEntry_{imguiID}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result}");
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

        Results = ConstructResults();
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
    public List<string> ConstructResults()
    {
        List<string> output = new();

        Dictionary<string, bool> Values = new();

        if (SearchText.Contains(" "))
        {
            var strValues = SearchText.Split(" ");
            foreach (var val in strValues)
            {
                if (!Values.ContainsKey(val))
                {
                    Values.Add(val, false);
                }
            }
        }
        else
        {
            if (!Values.ContainsKey(SearchText))
            {
                Values.Add(SearchText, false);
            }
        }

        foreach (var p in View.Editor.Project.Handler.ParamData.PrimaryBank.Params)
        {
            ProcessParam(ref output, p.Key, p.Value, Values);
        }

        return output;
    }

    public void ProcessParam(ref List<string> output, string paramKey, Andre.Formats.Param param, Dictionary<string, bool> values)
    {
        var tempTruthValues = new Dictionary<string, bool>(values);

        foreach (var row in param.Rows)
        {
            foreach (var entry in tempTruthValues)
            {
                if ($"{row.ID}" == entry.Key)
                {
                    tempTruthValues[entry.Key] = true;
                }
            }
        }

        if (tempTruthValues.All(x => x.Value == true))
        {
            output.Add(paramKey);
        }
    }
}

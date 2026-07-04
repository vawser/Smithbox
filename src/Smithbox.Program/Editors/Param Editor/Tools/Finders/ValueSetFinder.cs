using Hexa.NET.ImGui;
using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;


public class ValueSetFinder
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public string imguiID = "ValueSetFinder";

    public string SearchText = "";
    public string CachedSearchText = "";

    public List<FieldSearchResult> Results = new();

    public ValueSetFinder(ParamEditorScreen editor, ProjectEntry project)
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

        UIHelper.WrappedText(LOC.Get("PARAM_ValueSetFinder_Hint"));

        // Search Text
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("PARAM_DataFinder_Header_Search"),
            LOC.Get("PARAM_DataFinder_Header_Search_TT"));

        UIHelper.SetInputWidth();
        ImGui.InputTextWithHint($"{LOC.Get("PARAM_ValueSetFinder_Value_Set")}##searchText_{imguiID}", LOC.Get("PARAM_DataFinder_Search_Hint"), ref SearchText, 255);
        UIHelper.Tooltip(LOC.Get("PARAM_ValueSetFinder_Search_TT"));

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
            UIHelper.WrappedText(LOC.Get("PARAM_ValueSetFinder_Results_Column_Header"));

            ImGui.BeginChild($"##resultSection_{imguiID}",
                new Vector2(0, ImGui.GetContentRegionAvail().Y * 0.9f), ImGuiChildFlags.Borders);

            foreach (var result in Results)
            {
                var name = result.FieldInternalName;

                if (ImGui.Selectable($"{result.ParamName}: {name}##resultEntry_{imguiID}"))
                {
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
    public List<FieldSearchResult> ConstructResults()
    {
        List<FieldSearchResult> output = new();

        HashSet<string> Values = new HashSet<string>();

        if (SearchText.Contains(" "))
        {
            Values = SearchText.Split(" ").ToHashSet();
        }
        else
        {
            Values.Add(SearchText);
        }

        foreach (var p in Editor.Project.Handler.ParamData.PrimaryBank.Params)
        {
            ProcessParam(ref output, p.Key, p.Value, Values);
        }

        return output;
    }

    public void ProcessParam(ref List<FieldSearchResult> output, string paramKey, Andre.Formats.Param param, HashSet<string> values)
    {
        Dictionary<string, HashSet<string>> fieldValueSets = new();

        // Setup the dictionary
        foreach (var cell in param.Rows.First().Cells)
        {
            fieldValueSets.Add(cell.Def.InternalName, new HashSet<string>());
        }

        foreach (var row in param.Rows)
        {
            foreach (var cell in row.Cells)
            {
                var curValue = $"{cell.Value}";

                PARAMDEF.DefType type = cell.Def.DisplayType;

                foreach (var val in values)
                {
                    if (val == curValue)
                    {
                        if (fieldValueSets.ContainsKey(cell.Def.InternalName))
                        {
                            var curValSet = fieldValueSets[cell.Def.InternalName];

                            curValSet.Add(curValue);
                        }
                    }
                }
            }
        }

        foreach (var entry in fieldValueSets)
        {
            var curList = entry.Value;
            var listSize = curList.Count;

            if (entry.Value.SetEquals(values))
            {
                var newResult = new FieldSearchResult();
                newResult.ParamName = paramKey;
                newResult.FieldInternalName = entry.Key;

                output.Add(newResult);
            }
        }
    }
}

using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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

        var Size = ImGui.GetWindowSize();
        float EditX = (Size.X / 100) * 95;
        float EditY = (Size.Y / 100) * 25;

        UIHelper.WrappedText("Display fields that make use of a specified set of values.");
        UIHelper.WrappedText("");

        /// Search Configuration
        UIHelper.SimpleHeader("Search Configuration", "The configuration parameters for the search.");

        UIHelper.WrappedText("Search Value:");
        ImGui.InputText($"##searchValue_{imguiID}", ref SearchText, 255);
        UIHelper.Tooltip("The values to search for. Split with a space.");

        if (ImGui.Button($"Search##searchButton_{imguiID}"))
        {
            CachedSearchText = SearchText;

            Results = ConstructResults();
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

                if (ImGui.Selectable($"{result.ParamName}: {name}##resultEntry_{imguiID}"))
                {
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

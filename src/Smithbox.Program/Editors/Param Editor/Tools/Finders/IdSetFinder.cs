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
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public string imguiID = "IdSetFinder";

    public string SearchText = "";
    public string CachedSearchText = "";

    public List<string> Results = new();

    public IdSetFinder(ParamEditorScreen editor, ProjectEntry project)
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

        UIHelper.WrappedText("Display param that contains the specified set of rows.");
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
                if (ImGui.Selectable($"{result}##resultEntry_{imguiID}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result}");
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

        foreach (var p in Editor.Project.Handler.ParamData.PrimaryBank.Params)
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

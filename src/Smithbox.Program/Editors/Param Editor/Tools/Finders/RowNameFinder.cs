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


public class RowNameFinder
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public string imguiID = "RowNameFinder";

    public string SearchText = "";
    public string CachedSearchText = "";
    public int SearchIndex = -1;

    public List<string> TargetedParams = new();

    public List<DataSearchResult> Results = new();
    public RowNameFinder(ParamEditorScreen editor, ProjectEntry project)
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

        UIHelper.WrappedText("Display all instances of a specificed row name.");
        UIHelper.WrappedText("");

        /// Targeted Param
        UIHelper.SimpleHeader("Targeted Params", "Leave blank to target all params.");

        // Add
        if (ImGui.Button($"{Icons.Plus}##paramTargetAdd_rowNameFinder"))
        {
            TargetedParams.Add("");
        }
        UIHelper.Tooltip("Add new param target input row.");

        ImGui.SameLine();

        // Remove
        if (TargetedParams.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_rowNameFinder"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
            }
            UIHelper.Tooltip("Remove last added param target input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_rowNameFinder"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
                UIHelper.Tooltip("Remove last added param target input row.");
            }
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##paramTargetReset_rowNameFinder"))
        {
            TargetedParams = new List<string>();
        }
        UIHelper.Tooltip("Reset param target input rows.");

        for (int i = 0; i < TargetedParams.Count; i++)
        {
            var curCommand = TargetedParams[i];
            var curText = curCommand;

            if (ImGui.InputText($"##paramTargetInput{i}_rowNameFinder", ref curText, 255))
            {
                TargetedParams[i] = curText;
            }
            UIHelper.Tooltip("The param target to include.");
        }

        UIHelper.WrappedText("");

        /// Search Configuration
        UIHelper.SimpleHeader("Search Configuration", "The configuration parameters for the search.");

        // Row Index
        UIHelper.WrappedText("Row Index:");
        ImGui.InputInt($"##rowIndex_{imguiID}", ref SearchIndex);

        UIHelper.Tooltip("The row index to search for. -1 for any");

        // Search Text
        UIHelper.WrappedText("Search Text:");
        ImGui.InputText($"##searchText_{imguiID}", ref SearchText, 255);
        UIHelper.Tooltip("The row name to search for. Matches loosely.");

        // Search Button
        if (ImGui.Button("Search##action_SearchForRowNames"))
        {
            CachedSearchText = SearchText;

            Results = ConstructResults();
            Results.Sort();
        }

        UIHelper.WrappedText("");

        // Result List
        if (Results.Count > 0)
        {
            UIHelper.SimpleHeader("Search Results", "The results of the last search performed.");

            UIHelper.WrappedText($"Search Term:");
            UIHelper.DisplayAlias(CachedSearchText);

            UIHelper.WrappedText($"Result Count:");
            UIHelper.DisplayAlias($"{Results.Count}");

            UIHelper.WrappedText($"");
            UIHelper.WrappedText($"Param: Row Name");

            ImGui.BeginChild($"##resultSection_{imguiID}",
                new Vector2(EditX, EditY));

            foreach (var result in Results)
            {
                if (ImGui.Selectable($"{result.ParamName}: {result.RowName}##resultEntry_{imguiID}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result.ParamName}/{result.RowID}");
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
    private List<DataSearchResult> ConstructResults()
    {
        List<DataSearchResult> output = new();

        var searchElements = SearchText.Split(" ");

        foreach (var p in Editor.Project.Handler.ParamData.PrimaryBank.Params)
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

                bool addResult = false;

                var rowName = "";

                foreach (var element in searchElements)
                {
                    if (r.Name != "" && r.Name != null)
                    {
                        var nameElements = r.Name.Split(" ");

                        rowName = r.Name;

                        foreach (var rowElement in nameElements)
                        {
                            if (rowElement.Contains(element) && (SearchIndex == -1 || SearchIndex == i))
                            {
                                addResult = true;
                            }
                        }
                    }
                }

                if (addResult)
                {
                    var result = new DataSearchResult();
                    result.ParamName = p.Key;
                    result.RowID = r.ID;
                    result.RowIndex = i;
                    result.RowName = rowName;

                    output.Add(result);
                }
            }
        }

        return output;
    }
}

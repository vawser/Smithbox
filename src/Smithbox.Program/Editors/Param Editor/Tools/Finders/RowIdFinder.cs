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

public class RowIdFinder
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public string imguiID = "RowIDFinder";

    public int SearchID = -1;
    public int SearchIndex = -1;
    public int CachedSearchID = -1;
    public bool IncludeDescriptionInSearch = true;
    public bool DisplayCommunityNameInResult = false;

    public List<string> TargetedParams = new();

    public List<DataSearchResult> Results = new();

    public RowIdFinder(ParamEditorScreen editor, ProjectEntry project)
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

        UIHelper.WrappedText("Display all instances of a specificed row ID.");
        UIHelper.WrappedText("");

        // Targeted Param
        UIHelper.SimpleHeader("Targeted Params", "Leave blank to target all params.");

        // Add
        if (ImGui.Button($"{Icons.Plus}##paramTargetAdd_rowIdFinder"))
        {
            TargetedParams.Add("");
        }
        UIHelper.Tooltip("Add new param target input row.");

        ImGui.SameLine();

        // Remove
        if (TargetedParams.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_rowIdFinder"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
            }
            UIHelper.Tooltip("Remove last added param target input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_rowIdFinder"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
                UIHelper.Tooltip("Remove last added param target input row.");
            }
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##paramTargetReset_rowIdFinder"))
        {
            TargetedParams = new List<string>();
        }
        UIHelper.Tooltip("Reset param target input rows.");

        for (int i = 0; i < TargetedParams.Count; i++)
        {
            var curCommand = TargetedParams[i];
            var curText = curCommand;

            ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.5f);
            if (ImGui.InputText($"##paramTargetInput{i}_rowIdFinder", ref curText, 255))
            {
                TargetedParams[i] = curText;
            }
            UIHelper.Tooltip("The param target to include.");
        }

        UIHelper.WrappedText("");

        // Search Text
        UIHelper.SimpleHeader("Search", "");

        // Row Index
        UIHelper.IntInput($"rowIndex_{imguiID}", ref SearchIndex, "Row Index");
        UIHelper.Tooltip("The row index to search for. -1 for any");

        // Search Text
        UIHelper.IntInput($"searchId_{imguiID}", ref SearchID, "Row ID");
        UIHelper.Tooltip("The row ID to search for.");

        UIHelper.MultiButtonInput("searchActions",
            "search", "Search", "", ConductSearch,
            "clearSearch", "Clear", "", ClearSearch);

        UIHelper.WrappedText("");

        // Result List
        if (Results.Count > 0)
        {
            UIHelper.SimpleHeader("Search Results", "The results of the last search performed.");

            UIHelper.WrappedText($"Search Term:");
            UIHelper.DisplayAlias($"{CachedSearchID}");

            UIHelper.WrappedText($"Result Count:");
            UIHelper.DisplayAlias($"{Results.Count}");

            UIHelper.WrappedText($"");
            UIHelper.WrappedText($"Param:");

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
            ImGui.Text("No results to display.");
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

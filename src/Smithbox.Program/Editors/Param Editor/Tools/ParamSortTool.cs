using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ParamSortTool
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public ParamSortTool(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void DisplayDropDown()
    {
        if (ImGui.MenuItem("Sort Rows"))
        {
            if (View.Selection.ActiveParamExists())
            {
                SortRows();
            }
        }
        GUI.Tooltip("This will sort the rows by ID. WARNING: this is not recommended as row index can be important.");
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader("Sort Rows"))
        {
            ImGui.BeginChild("SortRowSection", ImGuiChildFlags.Borders);

            GUI.WrappedText("Use this to sort the ordering of the rows within the currently selected param.");
            GUI.WrappedText("This tool will respective row-index ordering for params with multiple rows of the same ID.");

            GUI.Spacer();
            GUI.SimpleHeader("Actions", "");

            GUI.MultiButtonInput("sortActions",
                "sortRows", "Sort Rows", "", SortRows);

            ImGui.EndChild();
        }
    }

    public void SortRows()
    {
        if (View.Selection.ActiveParamExists())
        {
            var action = SortRowsAction(
                View,
                View.GetPrimaryBank(),
                View.Selection.GetActiveParam());

            View.Editor.ActionManager.ExecuteAction(action);

            Smithbox.Log(typeof(ParamRowTools), $"Param rows sorted for " +
                $"{View.Selection.GetActiveParam()}");
        }
    }

    public ReorderRowAction SortRowsAction(ParamEditorView curView, ParamBank bank, string paramName)
    {
        Param param = bank.Params[paramName];

        // OrderBy is a stable sort: rows that share the same ID retain their
        // original relative order, so no rows are merged or lost.
        List<Param.Row> sortedRows = param.Rows
            .OrderBy(r => r.ID)
            .ToList();

        // ReorderRowAction replaces the entire row list in one undoable step.
        // Pass -1 as the drop-target index to signal a full replacement.
        return new ReorderRowAction(param, sortedRows, fullReplace: true);
    }
}

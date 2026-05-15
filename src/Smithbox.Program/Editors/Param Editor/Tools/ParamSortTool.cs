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
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public ParamSortTool(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void DisplayDropDown()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (ImGui.MenuItem("Sort Rows"))
        {
            if (activeView.Selection.ActiveParamExists())
            {
                SortRows();
            }
        }
        UIHelper.Tooltip("This will sort the rows by ID. WARNING: this is not recommended as row index can be important.");
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader("Sort Rows"))
        {
            ImGui.BeginChild("SortRowSection", ImGuiChildFlags.Borders);

            UIHelper.WrappedText("Use this to sort the ordering of the rows within the currently selected param.");
            UIHelper.WrappedText("This tool will respective row-index ordering for params with multiple rows of the same ID.");

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Actions", "");

            UIHelper.MultiButtonInput("sortActions",
                "sortRows", "Sort Rows", "", SortRows);

            ImGui.EndChild();
        }
    }

    public void SortRows()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView.Selection.ActiveParamExists())
        {
            var action = SortRowsAction(
                activeView,
                activeView.GetPrimaryBank(),
                activeView.Selection.GetActiveParam());

            activeView.Editor.ActionManager.ExecuteAction(action);

            Smithbox.Log(typeof(ParamRowTools), $"Param rows sorted for " +
                $"{activeView.Selection.GetActiveParam()}");
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

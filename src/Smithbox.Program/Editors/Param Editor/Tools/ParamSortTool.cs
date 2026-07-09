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
        // Sort Rows
        if (ImGui.MenuItem($"{LOC.Get("PARAM_RowSort_Sort_Rows")}##sortRowsAction"))
        {
            if (View.Selection.ActiveParamExists())
            {
                SortRows();
            }
        }
        GUI.Tooltip(LOC.Get("PARAM_RowSort_Sort_Rows_TT"));
    }

    public void Display()
    {
        // Sort Rows
        if (ImGui.CollapsingHeader($"{LOC.Get("PARAM_RowSort_Header_Sort_Rows")}##sortRowsHeader"))
        {
            ImGui.BeginChild("SortRowSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("PARAM_RowSort_Sort_Rows_Hint"));

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowSort_Header_Actions"),
                LOC.Get("PARAM_RowSort_Header_Actions_TT"));

            GUI.MultiButtonInput("sortActions",
                "sortRows", 
                LOC.Get("PARAM_RowSort_Sort_Rows_Action"),
                LOC.Get("PARAM_RowSort_Sort_Rows_Action_TT"),
                SortRows);

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

            Smithbox.Log(typeof(ParamRowTools),
                LOC.Get("PARAM_RowSort_Log_Sort_Rows", View.Selection.GetActiveParam()));
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

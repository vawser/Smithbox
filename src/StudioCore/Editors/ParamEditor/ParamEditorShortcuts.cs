using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamEditorShortcuts
{
    private ParamEditorScreen Screen;

    public ParamEditorShortcuts(ParamEditorScreen screen)
    {
        Screen = screen;
    }

    public void Shortcuts()
    {
        // Export CSV: Names
        if (!ImGui.IsAnyItemActive() && 
            Screen._activeView._selection.RowSelectionExists() && 
            InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExportCSV_Names))
        {
            EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/2");
        }

        // Export CSV: Param
        if (!ImGui.IsAnyItemActive() &&
            Screen._activeView._selection.RowSelectionExists() &&
            InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExportCSV_Param))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditCSVExport/0");
        }

        // Export CSV: All Rows
        if (!ImGui.IsAnyItemActive() &&
            Screen._activeView._selection.RowSelectionExists() &&
            InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExportCSV_AllRows))
        {
            EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/AllRows");
        }

        // Export CSV: Modified Rows
        if (!ImGui.IsAnyItemActive() &&
            Screen._activeView._selection.RowSelectionExists() &&
            InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExportCSV_ModifiedRows))
        {
            EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/ModifiedRows");
        }

        // Export CSV: Selected Rows
        if (!ImGui.IsAnyItemActive() &&
            Screen._activeView._selection.RowSelectionExists() &&
            InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExportCSV_SelectedRows))
        {
            EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/SelectedRows");
        }

    }
}

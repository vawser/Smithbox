using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editors.GparamEditor.Data;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamFileListView
{
    private GparamEditorScreen Screen;
    private GparamFilters Filters;
    private GparamSelectionManager Selection;
    private GparamContextMenu ContextMenu;

    public GparamFileListView(GparamEditorScreen screen)
    {
        Screen = screen;
        Filters = screen.Filters;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for this view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Files##GparamFileList");

        ImGui.Separator();

        Filters.DisplayFileFilterSearch();

        ImGui.Separator();

        foreach (var (name, info) in GparamParamBank.ParamBank)
        {
            var alias = AliasUtils.GetGparamAliasName(info.Name);

            if (Filters.IsFileFilterMatch(name, alias))
            {
                ImGui.BeginGroup();

                // File row
                if (ImGui.Selectable($@" {info.Name}", info.Name == Selection._selectedGparamKey))
                {
                    Selection.SetFileSelection(info);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Selection.SelectGparamFile)
                {
                    Selection.SelectGparamFile = false;

                    Selection.SetFileSelection(info);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Selection.SelectGparamFile = true;
                }

                if (CFG.Current.Interface_Display_Alias_for_Gparam)
                {
                    UIHelper.DisplayAlias(alias);
                }

                ImGui.EndGroup();
            }

            ContextMenu.FileContextMenu(name, info);
        }

        ImGui.End();
    }
}

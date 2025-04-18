using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editors.EmevdEditor.Enums;
using StudioCore.EmevdEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the file selection, viewing and editing.
/// </summary>
public class EmevdFileView
{
    private EmevdEditorScreen Screen;
    private EmevdPropertyDecorator Decorator;
    private EmevdSelectionManager Selection;
    private EmevdFilters Filters;
    private EmevdContextMenu ContextMenu;

    public EmevdFileView(EmevdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        Filters = screen.Filters;
        ContextMenu = screen.ContextMenu;
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the file view
    /// </summary>
    public void Display()
    {
        // File List
        ImGui.Begin("Files##EventScriptFileList");
        Selection.SwitchWindowContext(EmevdEditorContext.File);

        Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("FileListSection");
        Selection.SwitchWindowContext(EmevdEditorContext.File);

        foreach (var (info, binder) in EmevdBank.ScriptBank)
        {
            var displayName = $"{info.Name}";
            var aliasName = AliasUtils.GetMapNameAlias(info.Name);

            if (Filters.IsFileFilterMatch(displayName, aliasName))
            {
                // Script row
                if (ImGui.Selectable(displayName, info.Name == Selection.SelectedScriptKey))
                {
                    Selection.SelectedScriptKey = info.Name;
                    Selection.SelectedFileInfo = info;
                    Selection.SelectedScript = binder;
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Selection.SelectNextScript)
                {
                    Selection.SelectNextScript = false;
                    Selection.SelectedScriptKey = info.Name;
                    Selection.SelectedFileInfo = info;
                    Selection.SelectedScript = binder;
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Selection.SelectNextScript = true;
                }

                // Only apply to selection
                if (Selection.SelectedScriptKey != "")
                {
                    if (Selection.SelectedScriptKey == displayName)
                    {
                        ContextMenu.FileContextMenu(info);
                    }
                }

                UIHelper.DisplayAlias(aliasName);
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }
}

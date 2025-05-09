﻿using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editors.EsdEditor.Enums;
using StudioCore.Editors.TalkEditor;
using StudioCore.Interface;
using StudioCore.TalkEditor;
using StudioCore.Utilities;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Handles the file selection, viewing and editing.
/// </summary>
public class EsdFileView
{
    private EsdEditorScreen Screen;
    private EsdPropertyDecorator Decorator;
    private EsdSelectionManager Selection;
    private EsdFilters Filters;
    private EsdContextMenu ContextMenu;

    public EsdFileView(EsdEditorScreen screen)
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
        ImGui.Begin("Files##TalkFileList");
        Selection.SwitchWindowContext(EsdEditorContext.File);

        Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("FileListSection");
        Selection.SwitchWindowContext(EsdEditorContext.File);

        foreach (var (info, binder) in Screen.Project.EsdBank.TalkBank)
        {
            var displayName = $"{info.Name}";
            var aliasName = AliasUtils.GetMapNameAlias(Screen.Project, info.Name);

            if (Filters.IsFileFilterMatch(displayName, aliasName))
            {
                // File row
                if (ImGui.Selectable($@" {displayName}", displayName == Selection._selectedBinderKey))
                {
                    Selection.ResetScript();
                    Selection.ResetStateGroup();
                    Selection.ResetStateGroupNode();

                    Selection.SetFile(info, binder);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Selection.SelectNextFile)
                {
                    Selection.SelectNextFile = false;
                    Selection.SetFile(info, binder);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Selection.SelectNextFile = true;
                }

                // Only apply to selection
                if (Selection._selectedBinderKey != "")
                {
                    if (Selection._selectedBinderKey == info.Name)
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

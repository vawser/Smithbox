using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools;

public class DeleteAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public DeleteAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry) && Editor.ViewportSelection.IsSelection())
        {
            ApplyDelete();
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.Selectable("Delete"))
        {
            ApplyDelete();
        }
        UIHelper.Tooltip($"Delete the currently selected map objects.\n\nShortcut: {KeyBindings.Current.CORE_DeleteSelectedEntry.HintText}");
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Delete", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
        {
            ApplyDelete();
        }
        UIHelper.Tooltip($"Delete the currently selected map objects.");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        // Not shown here
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyDelete()
    {
        if (Editor.ViewportSelection.IsSelection())
        {
            DeleteMapObjectsAction action = new(Editor,
            Editor.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList(), true);
            Editor.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}
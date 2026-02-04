using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamShortcuts
{
    public GparamEditorScreen Editor;
    public ProjectEntry Project;

    public GparamShortcuts(GparamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Monitor()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (!FocusManager.IsInGparamEditor())
            return;

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_GparamEditor_ToolWindow = !CFG.Current.Interface_GparamEditor_ToolWindow;
        }

        // Save
        if (InputManager.IsPressed(KeybindID.Save))
        {
            Editor.Save();
        }

        if (activeView != null)
        {
            // Undo
            if (activeView.ActionManager.CanUndo())
            {
                if (InputManager.IsPressed(KeybindID.Undo))
                {
                    activeView.ActionManager.UndoAction();
                }
            }

            // Redo
            if (activeView.ActionManager.CanRedo())
            {
                if (InputManager.IsPressed(KeybindID.Redo))
                {
                    activeView.ActionManager.RedoAction();
                }
            }

            // Duplicate
            if (InputManager.IsPressed(KeybindID.Duplicate))
            {
                activeView.ActionHandler.DuplicateValueRow();
            }

            // Delete
            if (InputManager.IsPressed(KeybindID.Delete))
            {
                activeView.ActionHandler.DeleteValueRow();
            }

            // Execute Quick Edit
            if (InputManager.IsPressed(KeybindID.GparamEditor_Execute_Quick_Edit))
            {
                activeView.QuickEditHandler.ExecuteQuickEdit();
            }

            // Generate Quick Edit
            if (InputManager.IsPressed(KeybindID.GparamEditor_Generate_Quick_Edit))
            {
                activeView.QuickEditHandler.GenerateQuickEditCommands();
            }

            // Clear Quick Edit
            if (InputManager.IsPressed(KeybindID.GparamEditor_Clear_Quick_Edit))
            {
                activeView.QuickEditHandler.ClearQuickEditCommands();
            }
        }
    }
}

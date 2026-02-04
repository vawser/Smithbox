using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialShortcuts
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public MaterialShortcuts(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Monitor()
    {
        if (!FocusManager.IsInMaterialEditor())
            return;

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_MaterialEditor_ToolWindow = !CFG.Current.Interface_MaterialEditor_ToolWindow;
        }

        if (InputManager.IsPressed(KeybindID.Save))
        {
            Editor.Save();
        }

        if (Editor.EditorActionManager.CanUndo())
        {
            if (InputManager.IsPressed(KeybindID.Undo))
            {
                Editor.EditorActionManager.UndoAction();
            }
        }

        if (Editor.EditorActionManager.CanRedo())
        {
            if (InputManager.IsPressed(KeybindID.Redo))
            {
                Editor.EditorActionManager.RedoAction();
            }
        }

        // Actions

    }
}
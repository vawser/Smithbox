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

        if (InputManager.IsPressed(InputAction.Save))
        {
            Editor.Save();
        }

        if (Editor.EditorActionManager.CanUndo())
        {
            if (InputManager.IsPressed(InputAction.Undo))
            {
                Editor.EditorActionManager.UndoAction();
            }
        }

        if (Editor.EditorActionManager.CanRedo())
        {
            if (InputManager.IsPressed(InputAction.Redo))
            {
                Editor.EditorActionManager.RedoAction();
            }
        }

        // Actions

    }
}
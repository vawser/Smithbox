using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActShortcuts
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActShortcuts(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Monitor()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Save))
        {
            Editor.Save();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SaveAll))
        {
            Editor.SaveAll();
        }

        if (Editor.EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
        {
            Editor.EditorActionManager.UndoAction();
        }

        if (Editor.EditorActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
        {
            Editor.EditorActionManager.UndoAction();
        }

        if (Editor.EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
        {
            Editor.EditorActionManager.RedoAction();
        }

        if (Editor.EditorActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
        {
            Editor.EditorActionManager.RedoAction();
        }

        /*
        if (!ViewportUsingKeyboard && !ImGui.GetIO().WantCaptureKeyboard)
        {
            // F key frames the selection
            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Frame_Selection_in_Viewport))
            {
                HashSet<Entity> selected = _selection.GetFilteredSelection<Entity>();
                var first = false;
                BoundingBox box = new();
                foreach (Entity s in selected)
                {
                    if (s.RenderSceneMesh != null)
                    {
                        if (!first)
                        {
                            box = s.RenderSceneMesh.GetBounds();
                            first = true;
                        }
                        else
                        {
                            box = BoundingBox.Combine(box, s.RenderSceneMesh.GetBounds());
                        }
                    }
                }

                if (first)
                {
                    Viewport.FrameBox(box);
                }
            }
        }
        */

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
        {
            Editor.ActionHandler.DetermineDeleteTarget();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            Editor.ActionHandler.DetermineDuplicateTarget();
        }
    }
}

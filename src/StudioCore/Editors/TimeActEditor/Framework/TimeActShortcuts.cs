using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.TimeActEditor.Actions;
using StudioCore.Editors.TimeActEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActShortcuts
{
    private ActionManager ActionManager;
    private TimeActEditorScreen Editor;
    private TimeActActionHandler ActionHandler;

    public TimeActShortcuts(TimeActEditorScreen screen)
    {
        Editor = screen;
        ActionManager = screen.EditorActionManager;
        ActionHandler = screen.ActionHandler;
    }

    public void Monitor()
    {
        if (ActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
        {
            ActionManager.UndoAction();
        }

        if (ActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
        {
            ActionManager.UndoAction();
        }

        if (ActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
        {
            ActionManager.RedoAction();
        }

        if (ActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
        {
            ActionManager.RedoAction();
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
            ActionHandler.DetermineDeleteTarget();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            ActionHandler.DetermineDuplicateTarget();
        }
    }
}

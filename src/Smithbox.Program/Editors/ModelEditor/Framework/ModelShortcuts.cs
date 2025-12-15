using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Scene;
using StudioCore.Scene.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelShortcuts
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public ModelShortcuts(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Monitor()
    {
        if (!Editor.ModelViewportView.ViewportUsingKeyboard && !ImGui.IsAnyItemActive())
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Save))
            {
                Editor.Save();
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

            // Actions
            Editor.DeleteAction.OnShortcut();

            // Editor.DuplicateAction.OnShortcut();
            // Editor.DeleteAction.OnShortcut();
            // Editor.RotateAction.OnShortcut();
            // Editor.ReorderAction.OnShortcut();
            // Editor.GotoAction.OnShortcut();
            // Editor.FrameAction.OnShortcut();
            // Editor.PullToCameraAction.OnShortcut();

            // Gizmos
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_GizmoTranslationMode))
            {
                Gizmos.Mode = Gizmos.GizmosMode.Translate;
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_GizmoRotationMode))
            {
                Gizmos.Mode = Gizmos.GizmosMode.Rotate;
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_GizmoOriginMode))
            {
                if (Gizmos.Origin == Gizmos.GizmosOrigin.World)
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.BoundingBox;
                }
                else if (Gizmos.Origin == Gizmos.GizmosOrigin.BoundingBox)
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.World;
                }
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_GizmoSpaceMode))
            {
                if (Gizmos.Space == Gizmos.GizmosSpace.Local)
                {
                    Gizmos.Space = Gizmos.GizmosSpace.World;
                }
                else if (Gizmos.Space == Gizmos.GizmosSpace.World)
                {
                    Gizmos.Space = Gizmos.GizmosSpace.Local;
                }
            }
        }
    }
}

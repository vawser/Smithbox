using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.ModelEditor.Core;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Scene;
using StudioCore.Scene.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor;

public class ModelShortcuts
{
    private ModelEditorScreen Editor;
    private ViewportActionManager EditorActionManager;
    private FileSelectionView Selection;
    private ModelResourceManager ResourceManager;
    private ModelActionHandler ActionHandler;

    public ModelShortcuts(ModelEditorScreen screen)
    {
        Editor = screen;
        Selection = screen.FileSelection;
        EditorActionManager = screen.EditorActionManager;
        ActionHandler = screen.ActionHandler;
    }

    public void Monitor()
    {
        if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
        {
            EditorActionManager.RedoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
        {
            EditorActionManager.RedoAction();
        }

        // Create
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_CreateNewEntry))
        {
            ActionHandler.CreateHandler();
        }

        // Duplicate
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            ActionHandler.DuplicateHandler();
        }

        // Delete
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
        {
            ActionHandler.DeleteHandler();
        }

        // Toggle Selection Outline
        if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_RenderOutline))
        {
            CFG.Current.Viewport_Enable_Selection_Outline = !CFG.Current.Viewport_Enable_Selection_Outline;
        }

        if (!Editor.ViewportUsingKeyboard && !ImGui.GetIO().WantCaptureKeyboard)
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_GizmoTranslationMode))
            {
                Gizmos.Mode = Gizmos.GizmosMode.Translate;
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_GizmoRotationMode))
            {
                Gizmos.Mode = Gizmos.GizmosMode.Rotate;
            }

            // Use home key to cycle between gizmos origin modes
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

            // F key frames the selection
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FrameSelection))
            {
                HashSet<Entity> selected = Editor._selection.GetFilteredSelection<Entity>();
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
                    Editor.Viewport.FrameBox(box);
                }
            }

            // Render settings
            if (InputTracker.GetControlShortcut(Key.Number1))
            {
                Editor.RenderScene.DrawFilter = RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character |
                                         RenderFilter.Region;
            }
            else if (InputTracker.GetControlShortcut(Key.Number2))
            {
                Editor.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character |
                                         RenderFilter.Region;
            }
            else if (InputTracker.GetControlShortcut(Key.Number3))
            {
                Editor.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Navmesh | RenderFilter.Object |
                                         RenderFilter.Character | RenderFilter.Region;
            }
        }
    }
}

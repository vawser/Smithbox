using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Renderer;
using Veldrid;

namespace StudioCore.Editors.MapEditor;

public class MapShortcuts
{
    public MapEditorScreen Editor;

    public MapShortcuts(MapEditorScreen screen)
    {
        Editor = screen;
    }

    public void Monitor()
    {
        // Keyboard shortcuts
        if (!Editor.MapViewportView.ViewportUsingKeyboard && !ImGui.IsAnyItemActive())
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
            Editor.CreateAction.OnShortcut();
            Editor.DuplicateAction.OnShortcut();
            Editor.DeleteAction.OnShortcut();
            Editor.DuplicateToMapAction.OnShortcut();
            Editor.RotateAction.OnShortcut();
            Editor.ScrambleAction.OnShortcut();
            Editor.ReplicateAction.OnShortcut();
            Editor.RenderTypeAction.OnShortcut();
            Editor.ReorderAction.OnShortcut();
            Editor.GotoAction.OnShortcut();
            Editor.GameVisibilityAction.OnShortcut();
            Editor.FrameAction.OnShortcut();
            Editor.PullToCameraAction.OnShortcut();
            Editor.EditorVisibilityAction.OnShortcut();
            Editor.SelectionOutlineAction.OnShortcut();
            Editor.AdjustToGridAction.OnShortcut();
            Editor.SelectAllAction.OnShortcut();
            Editor.EntityInfoAction.OnShortcut();

            // Tools
            Editor.MassEditTool.OnShortcut();
            Editor.DisplayGroupTool.OnShortcut();
            Editor.PrefabTool.OnShortcut();
            Editor.SelectionGroupTool.OnShortcut();
            Editor.SelectionGroupTool.OnShortcut();
            Editor.RotationCycleConfigTool.OnShortcut();
            Editor.MovementCycleConfigTool.OnShortcut();

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

            // Render settings
            if (Editor.MapViewportView.RenderScene != null)
            {
                if (InputTracker.GetControlShortcut(Key.Number1))
                {
                    Editor.MapViewportView.RenderScene.DrawFilter = RenderFilter.MapPiece | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Region;
                }
                else if (InputTracker.GetControlShortcut(Key.Number2))
                {
                    Editor.MapViewportView.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Region;
                }
                else if (InputTracker.GetControlShortcut(Key.Number3))
                {
                    Editor.MapViewportView.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Navmesh |
                                             RenderFilter.Object | RenderFilter.Character |
                                             RenderFilter.Region;
                }
                else if (InputTracker.GetControlShortcut(Key.Number4))
                {
                    Editor.MapViewportView.RenderScene.DrawFilter = RenderFilter.MapPiece | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Light;
                }
                else if (InputTracker.GetControlShortcut(Key.Number5))
                {
                    Editor.MapViewportView.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Light;
                }
                else if (InputTracker.GetControlShortcut(Key.Number6))
                {
                    Editor.MapViewportView.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Navmesh |
                                             RenderFilter.MapPiece | RenderFilter.Collision |
                                             RenderFilter.Navmesh | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Region |
                                             RenderFilter.Light;
                }

                CFG.Current.LastSceneFilter = Editor.MapViewportView.RenderScene.DrawFilter;
            }
        }
    }
}

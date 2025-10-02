using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Scene;
using StudioCore.Scene.Enums;
using Veldrid;

namespace StudioCore.Editors.MapEditor.Framework;

public class MapShortcuts
{
    public MapEditorScreen Editor;
    private ViewportActionManager EditorActionManager;
    private MapViewportView ViewportView;
    private ViewportSelection Selection;
    private MapActionHandler ActionHandler;

    public MapShortcuts(MapEditorScreen screen)
    {
        Editor = screen;
        ViewportView = screen.MapViewportView;
        EditorActionManager = screen.EditorActionManager;
        Selection = screen.ViewportSelection;
        ActionHandler = screen.ActionHandler;
    }

    public void Monitor()
    {
        // Keyboard shortcuts
        if (!ViewportView.ViewportUsingKeyboard && !ImGui.IsAnyItemActive())
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Save))
            {
                Editor.Save();
            }

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
            if (ViewportView.RenderScene != null)
            {
                if (InputTracker.GetControlShortcut(Key.Number1))
                {
                    ViewportView.RenderScene.DrawFilter = RenderFilter.MapPiece | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Region;
                }
                else if (InputTracker.GetControlShortcut(Key.Number2))
                {
                    ViewportView.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Region;
                }
                else if (InputTracker.GetControlShortcut(Key.Number3))
                {
                    ViewportView.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Navmesh |
                                             RenderFilter.Object | RenderFilter.Character |
                                             RenderFilter.Region;
                }
                else if (InputTracker.GetControlShortcut(Key.Number4))
                {
                    ViewportView.RenderScene.DrawFilter = RenderFilter.MapPiece | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Light;
                }
                else if (InputTracker.GetControlShortcut(Key.Number5))
                {
                    ViewportView.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Light;
                }
                else if (InputTracker.GetControlShortcut(Key.Number6))
                {
                    ViewportView.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Navmesh |
                                             RenderFilter.MapPiece | RenderFilter.Collision |
                                             RenderFilter.Navmesh | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Region |
                                             RenderFilter.Light;
                }

                CFG.Current.LastSceneFilter = ViewportView.RenderScene.DrawFilter;
            }
        }
    }
}

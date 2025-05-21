using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Editors.MapEditor.Helpers;
using StudioCore.Scene;
using StudioCore.Scene.Enums;
using System.Linq;
using System.Numerics;
using Veldrid;
using static StudioCore.Editors.MapEditor.Framework.MapActionHandler;

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
            var type = CFG.Current.MapEditor_Viewport_GridType;

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

            // Viewport Grid
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_LowerGrid))
            {
                var offset = CFG.Current.MapEditor_Viewport_Grid_Height;
                var increment = CFG.Current.MapEditor_Viewport_Grid_Height_Increment;
                offset = offset - increment;
                CFG.Current.MapEditor_Viewport_Grid_Height = offset;
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_RaiseGrid))
            {
                var offset = CFG.Current.MapEditor_Viewport_Grid_Height;
                var increment = CFG.Current.MapEditor_Viewport_Grid_Height_Increment;
                offset = offset + increment;
                CFG.Current.MapEditor_Viewport_Grid_Height = offset;
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_SetGridToSelectionHeight))
            {
                var tempList = Selection.GetFilteredSelection<MsbEntity>().ToList();
                if (tempList != null && tempList.Count > 0)
                {
                    MsbEntity sel = tempList.First();
                    Vector3 pos = (Vector3)sel.GetPropertyValue("Position");
                    CFG.Current.MapEditor_Viewport_Grid_Height = pos.Y;
                }
            }

            // Create
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_CreateMapObject) && Selection.IsSelection())
            {
                ActionHandler.ApplyObjectCreation();
            }

            // Duplicate
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry) && Selection.IsSelection())
            {
                ActionHandler.ApplyDuplicate();
            }

            // Duplicate to Map
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DuplicateToMap) && Selection.IsSelection())
            {
                ImGui.OpenPopup("##DupeToTargetMapPopup");
            }

            // Delete
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry) && Selection.IsSelection())
            {
                ActionHandler.ApplyDelete();
            }

            // Frame in Viewport
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FrameSelection) && Selection.IsSelection())
            {
                ActionHandler.ApplyFrameInViewport();
            }

            // Go to in Map Object List
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_GoToInList) && Selection.IsSelection())
            {
                ActionHandler.ApplyGoToInObjectList();
            }

            // Move to Camera
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveToCamera) && Selection.IsSelection())
            {
                ActionHandler.ApplyMoveToCamera();
            }

            // Rotate (X-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_RotateSelectionXAxis))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
            }

            // Rotate (Y-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_RotateSelectionYAxis))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
            }

            // Rotate Pivot (Y-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_PivotSelectionYAxis))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
            }

            // Negative Rotate (X-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_NegativeRotateSelectionXAxis))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(-1, 0, 0), false);
            }

            // Negative Rotate (Y-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_NegativeRotateSelectionYAxis))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, -1, 0), false);
            }

            // Negative Rotate Pivot (Y-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_NegativePivotSelectionYAxis))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, -1, 0), true);
            }
            // Rotate (Fixed Increment)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_RotateFixedAngle))
            {
                ActionHandler.SetSelectionToFixedRotation(CFG.Current.Toolbar_Rotate_FixedAngle);
            }

            // Reset Rotation
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ResetRotation))
            {
                ActionHandler.SetSelectionToFixedRotation(new Vector3(0, 0, 0));
            }

            // Order (Up)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectUp) && Selection.IsSelection())
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Up);
            }

            // Order (Down)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectDown) && Selection.IsSelection())
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Down);
            }

            // Order (Top)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectTop) && Selection.IsSelection())
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Top);
            }

            // Order (Bottom)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectBottom) && Selection.IsSelection())
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Bottom);
            }

            // Scramble
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ScrambleSelection) && Selection.IsSelection())
            {
                ActionHandler.ApplyScramble();
            }

            // Replicate
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ReplicateSelection) && Selection.IsSelection())
            {
                ActionHandler.ApplyReplicate();
            }

            // Move to Grid
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SetSelectionToGrid) && Selection.IsSelection())
            {
                ActionHandler.ApplyMovetoGrid();
            }

            // Toggle Editor Visibility
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FlipSelectionVisibility) && Selection.IsSelection())
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableSelectionVisibility) && Selection.IsSelection())
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableSelectionVisibility) && Selection.IsSelection())
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FlipAllVisibility))
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableAllVisibility))
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableAllVisibility))
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Disable);
            }

            // Toggle In-game Visibility
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MakeDummyObject) && Selection.IsSelection())
            {
                ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MakeNormalObject) && Selection.IsSelection())
            {
                ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableGamePresence) && Selection.IsSelection())
            {
                ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableGamePresence) && Selection.IsSelection())
            {
                ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Enable);
            }

            // Toggle Selection Outline
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_RenderOutline))
            {
                CFG.Current.Viewport_Enable_Selection_Outline = !CFG.Current.Viewport_Enable_Selection_Outline;
            }

            // Toggle Render Type
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_ToggleRenderType))
            {
                VisualizationHelper.ToggleRenderType(Editor, Selection);
            }

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

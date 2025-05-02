using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Scene;
using StudioCore.Scene.Enums;
using System.Linq;
using System.Numerics;
using Veldrid;
using static StudioCore.Editors.MapEditorNS.MapActionHandler;

namespace StudioCore.Editors.MapEditorNS;
public class MapShortcuts
{
    public MapEditor Editor;

    public MapShortcuts(MapEditor editor)
    {
        Editor = editor;
    }

    public void Monitor()
    {
        // Keyboard shortcuts
        if (!Editor.MapViewport.ViewportUsingKeyboard && !ImGui.IsAnyItemActive())
        {
            var type = CFG.Current.MapEditor_Viewport_GridType;

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
                var tempList = Editor.Selection.GetFilteredSelection<MsbEntity>().ToList();
                if (tempList != null && tempList.Count > 0)
                {
                    MsbEntity sel = tempList.First();
                    Vector3 pos = (Vector3)sel.GetPropertyValue("Position");
                    CFG.Current.MapEditor_Viewport_Grid_Height = pos.Y;
                }
            }

            // Create
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_CreateMapObject) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyObjectCreation();
            }

            // Duplicate
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyDuplicate();
            }

            // Duplicate to Map
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DuplicateToMap) && Editor.Selection.IsSelection())
            {
                ImGui.OpenPopup("##DupeToTargetMapPopup");
            }

            // Delete
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyDelete();
            }

            // Frame in Viewport
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FrameSelection) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyFrameInViewport();
            }

            // Go to in Map Object List
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_GoToInList) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyGoToInObjectList();
            }

            // Move to Camera
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveToCamera) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyMoveToCamera();
            }

            // Rotate (X-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_RotateSelectionXAxis))
            {
                Editor.ActionHandler.ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
            }

            // Rotate (Y-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_RotateSelectionYAxis))
            {
                Editor.ActionHandler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
            }

            // Rotate Pivot (Y-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_PivotSelectionYAxis))
            {
                Editor.ActionHandler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
            }

            // Negative Rotate (X-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_NegativeRotateSelectionXAxis))
            {
                Editor.ActionHandler.ArbitraryRotation_Selection(new Vector3(-1, 0, 0), false);
            }

            // Negative Rotate (Y-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_NegativeRotateSelectionYAxis))
            {
                Editor.ActionHandler.ArbitraryRotation_Selection(new Vector3(0, -1, 0), false);
            }

            // Negative Rotate Pivot (Y-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_NegativePivotSelectionYAxis))
            {
                Editor.ActionHandler.ArbitraryRotation_Selection(new Vector3(0, -1, 0), true);
            }
            // Rotate (Fixed Increment)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_RotateFixedAngle))
            {
                Editor.ActionHandler.SetSelectionToFixedRotation(CFG.Current.Toolbar_Rotate_FixedAngle);
            }

            // Reset Rotation
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ResetRotation))
            {
                Editor.ActionHandler.SetSelectionToFixedRotation(new Vector3(0, 0, 0));
            }

            // Order (Up)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectUp) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Up);
            }

            // Order (Down)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectDown) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Down);
            }

            // Order (Top)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectTop) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Top);
            }

            // Order (Bottom)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectBottom) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Bottom);
            }

            // Scramble
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ScrambleSelection) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyScramble();
            }

            // Replicate
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ReplicateSelection) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyReplicate();
            }

            // Move to Grid
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SetSelectionToGrid) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyMovetoGrid();
            }

            // Toggle Editor Visibility
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FlipSelectionVisibility) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableSelectionVisibility) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableSelectionVisibility) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FlipAllVisibility))
            {
                Editor.ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableAllVisibility))
            {
                Editor.ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableAllVisibility))
            {
                Editor.ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Disable);
            }

            // Toggle In-game Visibility
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MakeDummyObject) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MakeNormalObject) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableGamePresence) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableGamePresence) && Editor.Selection.IsSelection())
            {
                Editor.ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Enable);
            }

            // Toggle Selection Outline
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_RenderOutline))
            {
                CFG.Current.Viewport_Enable_Selection_Outline = !CFG.Current.Viewport_Enable_Selection_Outline;
            }

            // Toggle Render Type
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_ToggleRenderType))
            {
                VisualizationHelper.ToggleRenderType(Editor.Selection);
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
            if (Editor.RenderScene != null)
            {
                if (InputTracker.GetControlShortcut(Key.Number1))
                {
                    Editor.RenderScene.DrawFilter = RenderFilter.MapPiece | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Region;
                }
                else if (InputTracker.GetControlShortcut(Key.Number2))
                {
                    Editor.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Region;
                }
                else if (InputTracker.GetControlShortcut(Key.Number3))
                {
                    Editor.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Navmesh |
                                             RenderFilter.Object | RenderFilter.Character |
                                             RenderFilter.Region;
                }
                else if (InputTracker.GetControlShortcut(Key.Number4))
                {
                    Editor.RenderScene.DrawFilter = RenderFilter.MapPiece | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Light;
                }
                else if (InputTracker.GetControlShortcut(Key.Number5))
                {
                    Editor.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Light;
                }
                else if (InputTracker.GetControlShortcut(Key.Number6))
                {
                    Editor.RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Navmesh |
                                             RenderFilter.MapPiece | RenderFilter.Collision |
                                             RenderFilter.Navmesh | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Region |
                                             RenderFilter.Light;
                }

                CFG.Current.LastSceneFilter = Editor.RenderScene.DrawFilter;
            }
        }
    }
}

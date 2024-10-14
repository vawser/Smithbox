using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Linq;
using System.Numerics;
using static StudioCore.Editors.MapEditor.Actions.ActionHandler;

namespace StudioCore.Editors.MapEditor.Actions;

public class ActionSubMenu
{
    private MapEditorScreen Screen;
    private ActionHandler Handler;

    public ActionSubMenu(MapEditorScreen screen, ActionHandler handler)
    {
        Screen = screen;
        Handler = handler;
    }

    public void Shortcuts()
    {
        // Create
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_CreateMapObject) && Screen._selection.IsSelection())
        {
            Handler.ApplyObjectCreation();
        }

        // Duplicate
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry) && Screen._selection.IsSelection())
        {
            Handler.ApplyDuplicate();
        }

        // Duplicate to Map
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DuplicateToMap) && Screen._selection.IsSelection())
        {
            ImGui.OpenPopup("##DupeToTargetMapPopup");
        }

        // Delete
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry) && Screen._selection.IsSelection())
        {
            Handler.ApplyDelete();
        }

        // Frame in Viewport
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FrameSelection) && Screen._selection.IsSelection())
        {
            Handler.ApplyFrameInViewport();
        }

        // Go to in Map Object List
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_GoToInList) && Screen._selection.IsSelection())
        {
            Handler.ApplyGoToInObjectList();
        }

        // Move to Camera
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveToCamera) && Screen._selection.IsSelection())
        {
            Handler.ApplyMoveToCamera();
        }

        // Rotate (X-axis)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_RotateSelectionXAxis))
        {
            Handler.ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
        }

        // Rotate (Y-axis)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_RotateSelectionYAxis))
        {
            Handler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
        }

        // Rotate Pivot (Y-axis)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_PivotSelectionYAxis))
        {
            Handler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
        }

        // Rotate (Fixed Increment)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_RotateFixedAngle))
        {
            Handler.SetSelectionToFixedRotation(CFG.Current.Toolbar_Rotate_FixedAngle);
        }

        // Reset Rotation
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ResetRotation))
        {
            Handler.SetSelectionToFixedRotation(new Vector3(0, 0, 0));
        }
        
        // Order (Up)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectUp) && Screen._selection.IsSelection())
        {
            Handler.ApplyMapObjectOrderChange(OrderMoveDir.Up);
        }

        // Order (Down)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectDown) && Screen._selection.IsSelection())
        {
            Handler.ApplyMapObjectOrderChange(OrderMoveDir.Down);
        }

        // Order (Top)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectTop) && Screen._selection.IsSelection())
        {
            Handler.ApplyMapObjectOrderChange(OrderMoveDir.Top);
        }

        // Order (Bottom)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectBottom) && Screen._selection.IsSelection())
        {
            Handler.ApplyMapObjectOrderChange(OrderMoveDir.Bottom);
        }

        // Scramble
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ScrambleSelection) && Screen._selection.IsSelection())
        {
            Handler.ApplyScramble();
        }

        // Replicate
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ReplicateSelection) && Screen._selection.IsSelection())
        {
            Handler.ApplyReplicate();
        }

        // Move to Grid
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SetSelectionToGrid) && Screen._selection.IsSelection())
        {
            Handler.ApplyMovetoGrid();
        }

        // Toggle Editor Visibility
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FlipSelectionVisibility) && Screen._selection.IsSelection())
        {
            Handler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableSelectionVisibility) && Screen._selection.IsSelection())
        {
            Handler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableSelectionVisibility) && Screen._selection.IsSelection())
        {
            Handler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FlipAllVisibility))
        {
            Handler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableAllVisibility))
        {
            Handler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableAllVisibility))
        {
            Handler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Disable);
        }

        // Toggle In-game Visibility
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MakeDummyObject) && Screen._selection.IsSelection())
        {
            Handler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MakeNormalObject) && Screen._selection.IsSelection())
        {
            Handler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableGamePresence) && Screen._selection.IsSelection())
        {
            Handler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableGamePresence) && Screen._selection.IsSelection())
        {
            Handler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Enable);
        }

        // Toggle Selection Outline
        if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_RenderOutline))
        {
            CFG.Current.Viewport_Enable_Selection_Outline = !CFG.Current.Viewport_Enable_Selection_Outline;
        }
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            ///--------------------
            // Duplicate
            ///--------------------
            if (ImGui.Button("Duplicate", UI.MenuButtonWideSize))
            {
                Handler.ApplyDuplicate();
            }
            UIHelper.ShowHoverTooltip($"Duplicate the currently selected map objects.\n{KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText}");

            ///--------------------
            // Delete
            ///--------------------
            if (ImGui.Button("Delete", UI.MenuButtonWideSize))
            {
                Handler.ApplyDelete();
            }
            UIHelper.ShowHoverTooltip($"Delete the currently selected map objects.\n{KeyBindings.Current.CORE_DeleteSelectedEntry.HintText}");

            ///--------------------
            // Scramble
            ///--------------------
            if (ImGui.Button("Scramble", UI.MenuButtonWideSize))
            {
                Handler.ApplyScramble();
            }
            UIHelper.ShowHoverTooltip($"Apply the scramble configuration to the currently selected map objects.\n{KeyBindings.Current.MAP_ScrambleSelection.HintText}");

            ///--------------------
            // Replicate
            ///--------------------
            if (ImGui.Button("Replicate", UI.MenuButtonWideSize))
            {
                Handler.ApplyReplicate();
            }
            UIHelper.ShowHoverTooltip($"Apply the replicate configuration to the currently selected map objects.\n{KeyBindings.Current.MAP_ReplicateSelection.HintText}");

            ImGui.Separator();

            ///--------------------
            // Duplicate to Map
            ///--------------------
            if (ImGui.BeginMenu("Duplicate Selected to Map"))
            {
                Handler.DisplayDuplicateToMapMenu(false, true);

                ImGui.EndMenu();
            }
            UIHelper.ShowHoverTooltip($"Duplicate the selected map objects into another map.");

            ///--------------------
            // Create
            ///--------------------
            if (ImGui.BeginMenu("Create New Object"))
            {
                if (ImGui.BeginCombo("##Targeted Map", Handler._targetMap.Item1))
                {
                    foreach (var obj in Screen.Universe.LoadedObjectContainers)
                    {
                        if (obj.Value != null)
                        {
                            if (ImGui.Selectable(obj.Key))
                            {
                                Handler._targetMap = (obj.Key, obj.Value);
                                break;
                            }
                        }
                    }
                    ImGui.EndCombo();
                }

                if (Handler._targetMap != (null, null))
                {
                    var map = (MapContainer)Handler._targetMap.Item2;

                    if (ImGui.BeginMenu("Parts"))
                    {
                        foreach ((string, Type) p in Handler._partsClasses)
                        {
                            if (ImGui.Button($"{p.Item1}", UI.MenuButtonWideSize))
                            {
                                CFG.Current.Toolbar_Create_Part = true;
                                CFG.Current.Toolbar_Create_Region = false;
                                CFG.Current.Toolbar_Create_Event = false;

                                Handler._createPartSelectedType = p.Item2;
                                Handler.ApplyObjectCreation();
                            }
                        }

                        ImGui.EndMenu();
                    }
                    UIHelper.ShowHoverTooltip("Create a Part object.");

                    if (Handler._regionClasses.Count == 1)
                    {
                        if (ImGui.Button("Region", UI.MenuButtonWideSize))
                        {
                            CFG.Current.Toolbar_Create_Part = false;
                            CFG.Current.Toolbar_Create_Region = true;
                            CFG.Current.Toolbar_Create_Event = false;

                            Handler._createRegionSelectedType = Handler._regionClasses[0].Item2;
                            Handler.ApplyObjectCreation();
                        }
                    }
                    else
                    {
                        if (ImGui.BeginMenu("Regions"))
                        {
                            foreach ((string, Type) p in Handler._regionClasses)
                            {
                                if (ImGui.Button($"{p.Item1}", UI.MenuButtonWideSize))
                                {
                                    CFG.Current.Toolbar_Create_Part = false;
                                    CFG.Current.Toolbar_Create_Region = true;
                                    CFG.Current.Toolbar_Create_Event = false;

                                    Handler._createRegionSelectedType = p.Item2;
                                    Handler.ApplyObjectCreation();
                                }
                            }

                            ImGui.EndMenu();
                        }
                        UIHelper.ShowHoverTooltip("Create a Region object.");
                    }

                    if (ImGui.BeginMenu("Events"))
                    {
                        foreach ((string, Type) p in Handler._eventClasses)
                        {
                            if (ImGui.Button($"{p.Item1}", UI.MenuButtonWideSize))
                            {
                                CFG.Current.Toolbar_Create_Part = false;
                                CFG.Current.Toolbar_Create_Region = false;
                                CFG.Current.Toolbar_Create_Event = true;

                                Handler._createEventSelectedType = p.Item2;
                                Handler.ApplyObjectCreation();
                            }
                        }

                        ImGui.EndMenu();
                    }
                    UIHelper.ShowHoverTooltip("Create an Event object.");

                    if (ImGui.Button("Light", UI.MenuButtonWideSize))
                    {
                        CFG.Current.Toolbar_Create_Part = false;
                        CFG.Current.Toolbar_Create_Region = false;
                        CFG.Current.Toolbar_Create_Event = false;

                        if (map.BTLParents.Any())
                        {
                            Handler.ApplyObjectCreation();
                        }
                    }
                    UIHelper.ShowHoverTooltip("Create a BTL Light object.");
                }

                ImGui.EndMenu();
            }
            UIHelper.ShowHoverTooltip($"Create a new map object.");

            ImGui.Separator();

            ///--------------------
            // Frame in Viewport
            ///--------------------
            if (ImGui.Button("Frame Selected in Viewport", UI.MenuButtonWideSize))
            {
                Handler.ApplyFrameInViewport();
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_FrameSelection.HintText}");

            ///--------------------
            // Move to Grid
            ///--------------------
            if (ImGui.Button("Move Selected to Grid", UI.MenuButtonWideSize))
            {
                Handler.ApplyMovetoGrid();
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_ReplicateSelection.HintText}");

            ///--------------------
            // Move to Camera
            ///--------------------
            if (ImGui.Button("Move Selected to Camera", UI.MenuButtonWideSize))
            {
                Handler.ApplyMoveToCamera();
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_MoveToCamera.HintText}");

            ImGui.Separator();

            ///--------------------
            // Rotate (X-axis)
            ///--------------------
            if (ImGui.Button("Rotate Selected (X-axis)", UI.MenuButtonWideSize))
            {
                Handler.ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_RotateSelectionXAxis.HintText}");

            ///--------------------
            // Rotate (Y-axis)
            ///--------------------
            if (ImGui.Button("Rotate Selected (Y-axis)", UI.MenuButtonWideSize))
            {
                Handler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_RotateSelectionYAxis.HintText}");

            ///--------------------
            // Rotate Pivot (Y-axis)
            ///--------------------
            if (ImGui.Button("Rotate Selected with Pivot (Y-axis)", UI.MenuButtonWideSize))
            {
                Handler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_PivotSelectionYAxis.HintText}");

            ///--------------------
            // Rotate Fixed Increment
            ///--------------------
            if (ImGui.Button("Rotate Selected to Fixed Angle", UI.MenuButtonWideSize))
            {
                Handler.SetSelectionToFixedRotation(CFG.Current.Toolbar_Rotate_FixedAngle);
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_RotateFixedAngle.HintText}");

            ///--------------------
            // Reset Rotation
            ///--------------------
            if (ImGui.Button("Reset Selected Rotation", UI.MenuButtonWideSize))
            {
                Handler.SetSelectionToFixedRotation(new Vector3(0, 0, 0));
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_ResetRotation.HintText}");

            ImGui.Separator();

            ///--------------------
            // Go to in List
            ///--------------------
            if (ImGui.Button("Go to in List", UI.MenuButtonWideSize))
            {
                Handler.ApplyGoToInObjectList();
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_GoToInList.HintText}");

            ///--------------------
            // Order (Up)
            ///--------------------
            if (ImGui.Button("Move Selected Up in List", UI.MenuButtonWideSize))
            {
                Handler.ApplyMapObjectOrderChange(OrderMoveDir.Up);
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_MoveObjectUp.HintText}");

            ///--------------------
            // Order (Down)
            ///--------------------
            if (ImGui.Button("Move Selected Down in List", UI.MenuButtonWideSize))
            {
                Handler.ApplyMapObjectOrderChange(OrderMoveDir.Down);
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_MoveObjectDown.HintText}");

            ///--------------------
            // Order (Top)
            ///--------------------
            if (ImGui.Button("Move Selected to the List Top", UI.MenuButtonWideSize))
            {
                Handler.ApplyMapObjectOrderChange(OrderMoveDir.Top);
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_MoveObjectTop.HintText}");

            ///--------------------
            // Order (Bottom)
            ///--------------------
            if (ImGui.Button("Move Selected to the List Bottom", UI.MenuButtonWideSize))
            {
                Handler.ApplyMapObjectOrderChange(OrderMoveDir.Bottom);
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_MoveObjectBottom.HintText}");

            ImGui.Separator();

            ///--------------------
            // Toggle Editor Visibility
            ///--------------------
            if (ImGui.BeginMenu("Toggle Editor Visibility"))
            {
                if (ImGui.Button("Flip Visibility for Selected", UI.MenuButtonWideSize))
                {
                    Handler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
                }
                UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_FlipSelectionVisibility.HintText}");

                if (ImGui.Button("Enable Visibility for Selected", UI.MenuButtonWideSize))
                {
                    Handler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
                }
                UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_EnableSelectionVisibility.HintText}");

                if (ImGui.Button("Disable Visibility for Selected", UI.MenuButtonWideSize))
                {
                    Handler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
                }
                UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_DisableSelectionVisibility.HintText}");

                if (ImGui.Button("Flip Visibility for All", UI.MenuButtonWideSize))
                {
                    Handler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
                }
                UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_FlipAllVisibility.HintText}");

                if (ImGui.Button("Enable Visibility for All", UI.MenuButtonWideSize))
                {
                    Handler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
                }
                UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_EnableAllVisibility.HintText}");

                if (ImGui.Button("Disable Visibility for All", UI.MenuButtonWideSize))
                {
                    Handler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Disable);
                }
                UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_DisableAllVisibility.HintText}");

                ImGui.EndMenu();
            }

            ///--------------------
            // Toggle In-game Visibility
            ///--------------------
            if (ImGui.BeginMenu("Toggle In-Game Visibility"))
            {
                if (ImGui.Button("Make Selected Normal Object into Dummy Object", UI.MenuButtonWideSize))
                {
                    Handler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
                }
                UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_MakeDummyObject.HintText}");

                if (ImGui.Button("Make Selected Dummy Object into Normal Object", UI.MenuButtonWideSize))
                {
                    Handler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
                }
                UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_MakeNormalObject.HintText}");

                if (Smithbox.ProjectType is ProjectType.ER)
                {
                    if (ImGui.Button("Disable Game Presence of Selected", UI.MenuButtonWideSize))
                    {
                        Handler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
                    }
                    UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_DisableGamePresence.HintText}");

                    if (ImGui.Button("Enable Game Presence of Selected", UI.MenuButtonWideSize))
                    {
                        Handler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Enable);
                    }
                    UIHelper.ShowHoverTooltip($"{KeyBindings.Current.MAP_EnableGamePresence.HintText}");
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }
}

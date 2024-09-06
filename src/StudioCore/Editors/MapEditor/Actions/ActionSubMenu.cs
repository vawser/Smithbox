using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editor;
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
            // Create
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
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
                            if (ImGui.MenuItem($"{p.Item1}"))
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
                    ImguiUtils.ShowHoverTooltip("Create a Part object.");

                    if (Handler._regionClasses.Count == 1)
                    {
                        if (ImGui.MenuItem("Region"))
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
                                if (ImGui.MenuItem($"{p.Item1}"))
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
                        ImguiUtils.ShowHoverTooltip("Create a Region object.");
                    }

                    if (ImGui.BeginMenu("Events"))
                    {
                        foreach ((string, Type) p in Handler._eventClasses)
                        {
                            if (ImGui.MenuItem($"{p.Item1}"))
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
                    ImguiUtils.ShowHoverTooltip("Create an Event object.");

                    if (ImGui.MenuItem("Light"))
                    {
                        CFG.Current.Toolbar_Create_Part = false;
                        CFG.Current.Toolbar_Create_Region = false;
                        CFG.Current.Toolbar_Create_Event = false;

                        if (map.BTLParents.Any())
                        {
                            Handler.ApplyObjectCreation();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Create a BTL Light object.");
                }

                ImGui.EndMenu();
            }

            ///--------------------
            // Duplicate
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Duplicate Selected", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                Handler.ApplyDuplicate();
            }

            ///--------------------
            // Duplicate to Map
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Duplicate Selected to Map"))
            {
                Handler.DisplayDuplicateToMapMenu(false, true);
                ImGui.EndMenu();
            }

            ///--------------------
            // Delete
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Delete Selected", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                Handler.ApplyDelete();
            }

            ///--------------------
            // Scramble
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Scramble Selected", KeyBindings.Current.MAP_ScrambleSelection.HintText))
            {
                Handler.ApplyScramble();
            }

            ///--------------------
            // Replicate
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Replicate Selected", KeyBindings.Current.MAP_ReplicateSelection.HintText))
            {
                Handler.ApplyReplicate();
            }

            ImGui.Separator();

            ///--------------------
            // Frame in Viewport
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Frame Selected in Viewport", KeyBindings.Current.MAP_FrameSelection.HintText))
            {
                Handler.ApplyFrameInViewport();
            }

            ///--------------------
            // Go to in List
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Go to in List", KeyBindings.Current.MAP_GoToInList.HintText))
            {
                Handler.ApplyGoToInObjectList();
            }

            ///--------------------
            // Move to Grid
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Move Selected to Grid", KeyBindings.Current.MAP_ReplicateSelection.HintText))
            {
                Handler.ApplyMovetoGrid();
            }

            ///--------------------
            // Move to Camera
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Move Selected to Camera", KeyBindings.Current.MAP_MoveToCamera.HintText))
            {
                Handler.ApplyMoveToCamera();
            }

            ImGui.Separator();

            ///--------------------
            // Rotate (X-axis)
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Rotate Selected (X-axis)", KeyBindings.Current.MAP_RotateSelectionXAxis.HintText))
            {
                Handler.ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
            }

            ///--------------------
            // Rotate (Y-axis)
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Rotate Selected (Y-axis)", KeyBindings.Current.MAP_RotateSelectionYAxis.HintText))
            {
                Handler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
            }

            ///--------------------
            // Rotate Pivot (Y-axis)
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Rotate Selected with Pivot (Y-axis)", KeyBindings.Current.MAP_PivotSelectionYAxis.HintText))
            {
                Handler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
            }

            ///--------------------
            // Rotate Fixed Increment
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Rotate Selected to Fixed Angle", KeyBindings.Current.MAP_RotateFixedAngle.HintText))
            {
                Handler.SetSelectionToFixedRotation(CFG.Current.Toolbar_Rotate_FixedAngle);
            }

            ///--------------------
            // Reset Rotation
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Reset Selected Rotation", KeyBindings.Current.MAP_ResetRotation.HintText))
            {
                Handler.SetSelectionToFixedRotation(new Vector3(0, 0, 0));
            }

            ImGui.Separator();

            ///--------------------
            // Order (Up)
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Move Selected Up in List", KeyBindings.Current.MAP_MoveObjectUp.HintText))
            {
                Handler.ApplyMapObjectOrderChange(OrderMoveDir.Up);
            }

            ///--------------------
            // Order (Down)
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Move Selected Down in List", KeyBindings.Current.MAP_MoveObjectBottom.HintText))
            {
                Handler.ApplyMapObjectOrderChange(OrderMoveDir.Down);
            }

            ///--------------------
            // Order (Top)
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Move Selected to the List Top", KeyBindings.Current.MAP_MoveObjectTop.HintText))
            {
                Handler.ApplyMapObjectOrderChange(OrderMoveDir.Top);
            }

            ///--------------------
            // Order (Bottom)
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Move Selected to the List Bottom", KeyBindings.Current.MAP_MoveObjectBottom.HintText))
            {
                Handler.ApplyMapObjectOrderChange(OrderMoveDir.Bottom);
            }

            ImGui.Separator();

            ///--------------------
            // Toggle Editor Visibility
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Toggle Editor Visibility"))
            {
                if (ImGui.MenuItem("Flip Visibility for Selected", KeyBindings.Current.MAP_FlipSelectionVisibility.HintText))
                {
                    Handler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
                }
                if (ImGui.MenuItem("Enable Visibility for Selected", KeyBindings.Current.MAP_EnableSelectionVisibility.HintText))
                {
                    Handler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
                }
                if (ImGui.MenuItem("Disable Visibility for Selected", KeyBindings.Current.MAP_DisableSelectionVisibility.HintText))
                {
                    Handler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
                }

                if (ImGui.MenuItem("Flip Visibility for All", KeyBindings.Current.MAP_FlipAllVisibility.HintText))
                {
                    Handler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
                }
                if (ImGui.MenuItem("Enable Visibility for All", KeyBindings.Current.MAP_EnableAllVisibility.HintText))
                {
                    Handler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
                }
                if (ImGui.MenuItem("Disable Visibility for All", KeyBindings.Current.MAP_DisableAllVisibility.HintText))
                {
                    Handler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Disable);
                }

                ImGui.EndMenu();
            }

            ///--------------------
            // Toggle In-game Visibility
            ///--------------------
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Toggle In-Game Visibility"))
            {
                if (ImGui.MenuItem("Make Selected Normal Object into Dummy Object", KeyBindings.Current.MAP_MakeDummyObject.HintText))
                {
                    Handler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
                }
                if (ImGui.MenuItem("Make Selected Dummy Object into Normal Object", KeyBindings.Current.MAP_MakeNormalObject.HintText))
                {
                    Handler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
                }

                if (Smithbox.ProjectType is ProjectType.ER)
                {
                    if (ImGui.MenuItem("Disable Game Presence of Selected", KeyBindings.Current.MAP_DisableGamePresence.HintText))
                    {
                        Handler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
                    }
                    if (ImGui.MenuItem("Enable Game Presence of Selected", KeyBindings.Current.MAP_EnableGamePresence.HintText))
                    {
                        Handler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Enable);
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }
}

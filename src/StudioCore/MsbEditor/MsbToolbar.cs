using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using StudioCore.Aliases;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Gui;
using StudioCore.ParamEditor;
using StudioCore.Scene;
using StudioCore.Settings;
using StudioCore.Utilities;
using Veldrid;
using System.IO;
using System;
using System.Threading;
using static SoulsFormats.ACB;
using DotNext;
using Silk.NET.SDL;
using Veldrid.Utilities;
using SoulsFormats;

namespace StudioCore.MsbEditor
{
    public enum SelectedTool
    {
        None,
        Selection_ToggleVisibility,
        Selection_GoToInObjectList,
        Selection_MoveToCamera,
        Selection_FrameInViewport,
        Selection_Duplicate,
        Selection_Rotate,
        Selection_Dummify,
        Selection_Undummify,
        Selection_Move_to_Grid,
        Selection_Scramble,
        Selection_Replicate
    }

    public class MsbToolbar
    {
        private readonly ActionManager _actionManager;

        private readonly RenderScene _scene;
        private readonly Selection _selection;

        private AssetLocator _assetLocator;
        private MsbEditorScreen _msbEditor;

        private Universe _universe;

        private IViewport _viewport;

        private SelectedTool _selectedTool;

        public MsbToolbar(RenderScene scene, Selection sel, ActionManager manager, Universe universe, AssetLocator locator, MsbEditorScreen editor, IViewport viewport)
        {
            _scene = scene;
            _selection = sel;
            _actionManager = manager;
            _universe = universe;

            _assetLocator = locator;
            _msbEditor = editor;
            _viewport = viewport;
        }

        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (_assetLocator.Type == GameType.Undefined)
                return;

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Toolbar##MsbMenubar"))
            {
                ImGui.Columns(2);

                // Selection List
                ImGui.BeginChild("toolselection");

                ImGui.Text("Double-click to use.");
                ImGui.Separator();
                
                // Go to in Object List
                if (ImGui.Selectable("Go to in Object List##tool_Selection_GoToInObjectList", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedTool = SelectedTool.Selection_GoToInObjectList;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        GoToInObjectList();
                    }
                }

                // Move to Camera
                if (ImGui.Selectable("Move to Camera##tool_Selection_MoveToCamera", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedTool = SelectedTool.Selection_MoveToCamera;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        MoveSelectionToCamera();
                    }
                }

                // Frame in Viewport
                if (ImGui.Selectable("Frame in Viewport##tool_Selection_FrameInViewport", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedTool = SelectedTool.Selection_FrameInViewport;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        FrameSelection();
                    }
                }

                // Toggle Visibility
                if (ImGui.Selectable("Toggle Visibility##tool_Selection_ToggleVisibility", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedTool = SelectedTool.Selection_ToggleVisibility;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        ToggleEntityVisibility();
                    }
                }

                // Duplicate
                if (ImGui.Selectable("Duplicate##tool_Selection_Duplicate", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedTool = SelectedTool.Selection_Duplicate;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        DuplicateSelection();
                    }
                }

                // Rotate
                if (ImGui.Selectable("Rotate##tool_Selection_Rotate", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedTool = SelectedTool.Selection_Rotate;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        if (CFG.Current.Toolbar_Rotate_X)
                        {
                            ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
                        }
                        if (CFG.Current.Toolbar_Rotate_Y)
                        {
                            ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
                        }
                        if (CFG.Current.Toolbar_Rotate_Y_Pivot)
                        {
                            ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
                        }
                        if (CFG.Current.Toolbar_Fixed_Rotate)
                        {
                            SetSelectionToFixedRotation();
                        }
                    }
                }

                // Dummify
                if (ImGui.Selectable("Dummify##tool_Selection_Dummify", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedTool = SelectedTool.Selection_Dummify;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        DummySelection();
                    }
                }

                // Undummify
                if (ImGui.Selectable("Undummify##tool_Selection_Undummify", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedTool = SelectedTool.Selection_Undummify;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        UnDummySelection();
                    }
                }

                // Scramble
                if (ImGui.Selectable("Scramble##tool_Selection_Scramble", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedTool = SelectedTool.Selection_Scramble;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        ScambleSelection();
                    }
                }

                // Replicate
                if (ImGui.Selectable("Replicate##tool_Selection_Replicate", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedTool = SelectedTool.Selection_Replicate;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        ReplicateSelection();
                    }
                }

                // Move to Grid
                if (CFG.Current.Viewport_EnableGrid)
                {
                    if (ImGui.Selectable("Move to Grid##tool_Selection_Move_to_Grid", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedTool = SelectedTool.Selection_Move_to_Grid;

                        if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                        {
                            MoveSelectionToGrid();
                        }
                    }
                }

                ImGui.EndChild();

                // Configuration Window
                ImGui.NextColumn();

                ImGui.BeginChild("toolconfiguration");

                // Go to in Object List
                if (_selectedTool == SelectedTool.Selection_GoToInObjectList)
                {
                    ImGui.Text("Move the camera to the current selection (first if multiple are selected).");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Go_to_Selection_in_Object_List.HintText)}");
                    ImGui.Separator();
                }

                // Move to Camera
                if (_selectedTool == SelectedTool.Selection_MoveToCamera)
                {
                    ImGui.Text("Move the current selection to the camera position.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Move_Selection_to_Camera.HintText)}");
                    ImGui.Separator();


                    if (ImGui.Button("Switch"))
                    {
                        CFG.Current.Toolbar_Move_to_Camera_Offset_Specific_Input = !CFG.Current.Toolbar_Move_to_Camera_Offset_Specific_Input;
                    }
                    ImGui.SameLine();
                    if (CFG.Current.Toolbar_Move_to_Camera_Offset_Specific_Input)
                    {
                        var offset = CFG.Current.Toolbar_Move_to_Camera_Offset;

                        ImGui.PushItemWidth(200);
                        ImGui.InputFloat("Offset distance", ref offset);
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("Set the distance at which the current selection is offset from the camera when this action is used.");
                        }

                        if (offset < 0)
                            offset = 0;

                        if (offset > 100)
                            offset = 100;

                        CFG.Current.Toolbar_Move_to_Camera_Offset = offset;
                    }
                    else
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.SliderFloat("Offset distance", ref CFG.Current.Toolbar_Move_to_Camera_Offset, 0, 100);
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("Set the distance at which the current selection is offset from the camera when this action is used.");
                        }
                    }
                }

                // Frame in Viewport
                if (_selectedTool == SelectedTool.Selection_FrameInViewport)
                {
                    ImGui.Text("Frame the current selection in the viewport (first if multiple are selected).");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Frame_Selection_in_Viewport.HintText)}");
                    ImGui.Separator();
                }

                // Toggle Visibility
                if (_selectedTool == SelectedTool.Selection_ToggleVisibility)
                {
                    ImGui.Text("Toggle the visibility of the current selection or all objects.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Flip.HintText)} for Selection (Flip).");
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Flip.HintText)} for all Objects (Flip).");

                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Enabled.HintText)} for Selection (Enabled).");
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Enabled.HintText)} for all Objects (Enabled).");

                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Disabled.HintText)} for Selection (Disabled).");
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Disabled.HintText)} for all Objects (Disabled).");

                    ImGui.Separator();

                    ImGui.Text("Target:");
                    if (ImGui.Checkbox("Selection", ref CFG.Current.Toolbar_Visibility_Target_Selection))
                    {
                        CFG.Current.Toolbar_Visibility_Target_All = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Set the target state to our current selection.");
                    }
                    if (ImGui.Checkbox("All", ref CFG.Current.Toolbar_Visibility_Target_All))
                    {
                        CFG.Current.Toolbar_Visibility_Target_Selection = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Set the target state to all objects.");
                    }

                    ImGui.Separator();
                    ImGui.Text("State:");
                    if (ImGui.Checkbox("Visible", ref CFG.Current.Toolbar_Visibility_State_Enabled))
                    {
                        CFG.Current.Toolbar_Visibility_State_Disabled = false;
                        CFG.Current.Toolbar_Visibility_State_Flip = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Set the target selection visible state to enabled.");
                    }
                    if (ImGui.Checkbox("Invisible", ref CFG.Current.Toolbar_Visibility_State_Disabled))
                    {
                        CFG.Current.Toolbar_Visibility_State_Enabled = false;
                        CFG.Current.Toolbar_Visibility_State_Flip = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Set the target selection visible state to disabled.");
                    }
                    if (ImGui.Checkbox("Flip", ref CFG.Current.Toolbar_Visibility_State_Flip))
                    {
                        CFG.Current.Toolbar_Visibility_State_Enabled = false;
                        CFG.Current.Toolbar_Visibility_State_Disabled = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Set the target selection visible state to opposite of its current state.");
                    }
                }

                // Duplicate
                if (_selectedTool == SelectedTool.Selection_Duplicate)
                {
                    ImGui.Text("Duplicate the current selection.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Core_Duplicate.HintText)}");
                    ImGui.Separator();

                    if (_assetLocator.Type != GameType.DarkSoulsIISOTFS)
                    {
                        ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Toolbar_Duplicate_Increment_Entity_ID);

                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("When enabled, the duplicated entities will be given a new valid Entity ID.");
                        }
                    }

                    if (_assetLocator.Type == GameType.EldenRing)
                    {
                        ImGui.Checkbox("Increment UnkPartNames for Assets", ref CFG.Current.Toolbar_Duplicate_Increment_UnkPartNames);

                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("When enabled, the duplicated Asset entities UnkPartNames property will be updated.");
                        }
                    }
                }

                // Rotate
                if (_selectedTool == SelectedTool.Selection_Rotate)
                {
                    ImGui.Text("Rotate the current selection by the following parameters.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Rotate_X.HintText)} for Rotate X");
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Rotate_Y.HintText)} for Rotate Y");
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Rotate_Y_Pivot.HintText)} for Rotate Pivot Y");
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Reset_Rotation.HintText)} for Fixed Rotation");
                    ImGui.Separator();

                    var rot = CFG.Current.Toolbar_Rotate_Increment;

                    if(ImGui.Checkbox("X", ref CFG.Current.Toolbar_Rotate_X))
                    {
                        CFG.Current.Toolbar_Rotate_Y = false;
                        CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                        CFG.Current.Toolbar_Fixed_Rotate = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Set the rotation axis to X.");
                    }
                    ImGui.SameLine();
                    if (ImGui.Checkbox("Y", ref CFG.Current.Toolbar_Rotate_Y))
                    {
                        CFG.Current.Toolbar_Rotate_X = false;
                        CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                        CFG.Current.Toolbar_Fixed_Rotate = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Set the rotation axis to Y.");
                    }
                    ImGui.SameLine();
                    if (ImGui.Checkbox("Y Pivot", ref CFG.Current.Toolbar_Rotate_Y_Pivot))
                    {
                        CFG.Current.Toolbar_Rotate_Y = false;
                        CFG.Current.Toolbar_Rotate_X = false;
                        CFG.Current.Toolbar_Fixed_Rotate = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Set the rotation axis to Y and pivot with respect to others within the selection.");
                    }
                    ImGui.SameLine();
                    if (ImGui.Checkbox("Fixed Rotation", ref CFG.Current.Toolbar_Fixed_Rotate))
                    {
                        CFG.Current.Toolbar_Rotate_Y = false;
                        CFG.Current.Toolbar_Rotate_X = false;
                        CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Set the rotation axis to specified values below.");
                    }

                    if (ImGui.Button("Switch"))
                    {
                        CFG.Current.Toolbar_Rotate_Specific_Input = !CFG.Current.Toolbar_Rotate_Specific_Input;
                    }
                    ImGui.SameLine();
                    if (CFG.Current.Toolbar_Rotate_Specific_Input)
                    {
                        ImGui.PushItemWidth(200);
                        if (ImGui.InputFloat("Degree Increment", ref rot))
                        {
                            CFG.Current.Toolbar_Rotate_Increment = Math.Clamp(rot, -180.0f, 180.0f);
                        }
                    }
                    else
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.SliderFloat("Degree Increment", ref rot, -180.0f, 180.0f);
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Set the angle increment amount used by the rotation.");
                    }

                    var x = CFG.Current.Toolbar_Rotate_FixedAngle[0];
                    var y = CFG.Current.Toolbar_Rotate_FixedAngle[1];
                    var z = CFG.Current.Toolbar_Rotate_FixedAngle[2];

                    ImGui.Text("Fixed Rotation");
                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("X##fixedRotationX", ref x))
                    {
                        x = Math.Clamp(x, -360f, 360f);
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Set the X component of the fixed rotation action.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("Y##fixedRotationX", ref y))
                    {
                        y = Math.Clamp(y, -360f, 360f);
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Set the Y component of the fixed rotation action.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("Z##fixedRotationZ", ref z))
                    {
                        z = Math.Clamp(z, -360f, 360f);
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Set the Z component of the fixed rotation action.");
                    }
                    ImGui.SameLine();

                    CFG.Current.Toolbar_Rotate_FixedAngle = new Vector3(x, y, z);
                }

                // Dummify
                if (_selectedTool == SelectedTool.Selection_Dummify)
                {
                    ImGui.Text("Make the current selection Dummy Enemy/Object/Asset entities.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Dummify.HintText)}");
                    ImGui.Separator();
                }

                // Undummify
                if (_selectedTool == SelectedTool.Selection_Undummify)
                {
                    ImGui.Text("Make the current selection normal Enemy/Object/Asset entities.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Undummify.HintText)}");
                    ImGui.Separator();
                }

                // Move to Grid
                if (_selectedTool == SelectedTool.Selection_Move_to_Grid)
                {
                    ImGui.Text("Set the current selection to the closest grid position.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Set_to_Grid.HintText)}");
                    ImGui.Separator();

                    ImGui.Checkbox("X", ref CFG.Current.Toolbar_Move_to_Grid_X);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Move the current selection to the closest X co-ordinate within the map grid.");
                    }
                    ImGui.SameLine();
                    ImGui.Checkbox("Y", ref CFG.Current.Toolbar_Move_to_Grid_Y);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Move the current selection to the closest Y co-ordinate within the map grid.");
                    }
                    ImGui.SameLine();
                    ImGui.Checkbox("Z", ref CFG.Current.Toolbar_Move_to_Grid_Z);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Move the current selection to the closest Z co-ordinate within the map grid.");
                    }

                    if (ImGui.Button("Switch"))
                    {
                        CFG.Current.Toolbar_Move_to_Grid_Specific_Height_Input = !CFG.Current.Toolbar_Move_to_Grid_Specific_Height_Input;
                    }
                    ImGui.SameLine();
                    if (CFG.Current.Toolbar_Move_to_Grid_Specific_Height_Input)
                    {
                        var height = CFG.Current.Viewport_Grid_Height;

                        ImGui.PushItemWidth(200);
                        ImGui.InputFloat("Grid height", ref height);
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("Set the current height of the map grid.");
                        }

                        if (height < -10000)
                            height = -10000;

                        if (height > 10000)
                            height = 10000;

                        CFG.Current.Viewport_Grid_Height = height;
                    }
                    else
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.SliderFloat("Grid height", ref CFG.Current.Viewport_Grid_Height, -10000, 10000);
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("Set the current height of the map grid.");
                        }
                    }
                }

                // Scramble
                if (_selectedTool == SelectedTool.Selection_Scramble)
                {
                    ImGui.Text("Scramble the current selection's position, rotation and scale by the following parameters.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Scramble.HintText)}");
                    ImGui.Separator();

                    var randomOffsetMin_Pos_X = CFG.Current.Scrambler_OffsetMin_Position_X;
                    var randomOffsetMin_Pos_Y = CFG.Current.Scrambler_OffsetMin_Position_Y;
                    var randomOffsetMin_Pos_Z = CFG.Current.Scrambler_OffsetMin_Position_Z;

                    var randomOffsetMax_Pos_X = CFG.Current.Scrambler_OffsetMax_Position_X;
                    var randomOffsetMax_Pos_Y = CFG.Current.Scrambler_OffsetMax_Position_Y;
                    var randomOffsetMax_Pos_Z = CFG.Current.Scrambler_OffsetMax_Position_Z;

                    var randomOffsetMin_Rot_X = CFG.Current.Scrambler_OffsetMin_Rotation_X;
                    var randomOffsetMin_Rot_Y = CFG.Current.Scrambler_OffsetMin_Rotation_Y;
                    var randomOffsetMin_Rot_Z = CFG.Current.Scrambler_OffsetMin_Rotation_Z;

                    var randomOffsetMax_Rot_X = CFG.Current.Scrambler_OffsetMax_Rotation_X;
                    var randomOffsetMax_Rot_Y = CFG.Current.Scrambler_OffsetMax_Rotation_Y;
                    var randomOffsetMax_Rot_Z = CFG.Current.Scrambler_OffsetMax_Rotation_Z;

                    var randomOffsetMin_Scale_X = CFG.Current.Scrambler_OffsetMin_Scale_X;
                    var randomOffsetMin_Scale_Y = CFG.Current.Scrambler_OffsetMin_Scale_Y;
                    var randomOffsetMin_Scale_Z = CFG.Current.Scrambler_OffsetMin_Scale_Z;

                    var randomOffsetMax_Scale_X = CFG.Current.Scrambler_OffsetMax_Scale_X;
                    var randomOffsetMax_Scale_Y = CFG.Current.Scrambler_OffsetMax_Scale_Y;
                    var randomOffsetMax_Scale_Z = CFG.Current.Scrambler_OffsetMax_Scale_Z;

                    // Position
                    ImGui.Text("Position");
                    ImGui.Checkbox("X##scramblePosX", ref CFG.Current.Scrambler_RandomisePosition_X);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Include the X co-ordinate of the selection's Position in the scramble.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinPosX", ref randomOffsetMin_Pos_X);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Minimum amount to add to the position X co-ordinate.");
                    }
                    ImGui.SameLine();

                    ImGui.InputFloat("Upper Bound##offsetMaxPosX", ref randomOffsetMax_Pos_X);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Maximum amount to add to the position X co-ordinate.");
                    }

                    ImGui.Checkbox("Y##scramblePosY", ref CFG.Current.Scrambler_RandomisePosition_Y);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Include the Y co-ordinate of the selection's Position in the scramble.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinPosY", ref randomOffsetMin_Pos_Y);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Minimum amount to add to the position Y co-ordinate.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxPosY", ref randomOffsetMax_Pos_Y);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Maximum amount to add to the position Y co-ordinate.");
                    }

                    ImGui.Checkbox("Z##scramblePosZ", ref CFG.Current.Scrambler_RandomisePosition_Z);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Include the Z co-ordinate of the selection's Position in the scramble.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinPosZ", ref randomOffsetMin_Pos_Z);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Minimum amount to add to the position Z co-ordinate.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxPosZ", ref randomOffsetMax_Pos_Z);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Maximum amount to add to the position Z co-ordinate.");
                    }

                    // Rotation
                    ImGui.Text("Rotation");
                    ImGui.Checkbox("X##scrambleRotX", ref CFG.Current.Scrambler_RandomiseRotation_X);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Include the X co-ordinate of the selection's Rotation in the scramble.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinRotX", ref randomOffsetMin_Rot_X);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Minimum amount to add to the rotation X co-ordinate.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxRotX", ref randomOffsetMax_Rot_X);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Maximum amount to add to the rotation X co-ordinate.");
                    }

                    ImGui.Checkbox("Y##scrambleRotY", ref CFG.Current.Scrambler_RandomiseRotation_Y);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Include the Y co-ordinate of the selection's Rotation in the scramble.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinRotY", ref randomOffsetMin_Rot_Y);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Minimum amount to add to the rotation Y co-ordinate.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxRotY", ref randomOffsetMax_Rot_Y);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Maximum amount to add to the rotation Y co-ordinate.");
                    }

                    ImGui.Checkbox("Z##scrambleRotZ", ref CFG.Current.Scrambler_RandomiseRotation_Z);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Include the Z co-ordinate of the selection's Rotation in the scramble.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinRotZ", ref randomOffsetMin_Rot_Z);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Minimum amount to add to the rotation Z co-ordinate.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxRotZ", ref randomOffsetMax_Rot_Z);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Maximum amount to add to the rotation Z co-ordinate.");
                    }

                    // Scale
                    ImGui.Text("Scale");
                    ImGui.Checkbox("X##scrambleScaleX", ref CFG.Current.Scrambler_RandomiseScale_X);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Include the X co-ordinate of the selection's Scale in the scramble.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinScaleX", ref randomOffsetMin_Scale_X);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Minimum amount to add to the scale X co-ordinate.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxScaleX", ref randomOffsetMax_Scale_X);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Maximum amount to add to the scale X co-ordinate.");
                    }

                    ImGui.Checkbox("Y##scrambleScaleY", ref CFG.Current.Scrambler_RandomiseScale_Y);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Include the Y co-ordinate of the selection's Scale in the scramble.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinScaleY", ref randomOffsetMin_Scale_Y);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Minimum amount to add to the scale Y co-ordinate.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxScaleY", ref randomOffsetMax_Scale_Y);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Maximum amount to add to the scale Y co-ordinate.");
                    }

                    ImGui.Checkbox("Z##scrambleScaleZ", ref CFG.Current.Scrambler_RandomiseScale_Z);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Include the Z co-ordinate of the selection's Scale in the scramble.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinScaleZ", ref randomOffsetMin_Scale_Z);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Minimum amount to add to the scale Z co-ordinate.");
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxScaleZ", ref randomOffsetMax_Scale_Y);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Maximum amount to add to the scale Z co-ordinate.");
                    }

                    ImGui.Checkbox("Scale Proportionally##scrambleSharedScale", ref CFG.Current.Scrambler_RandomiseScale_SharedScale);
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("When scrambling the scale, the Y and Z values will follow the X value, making the scaling proportional.");
                    }

                    // Clamp floats
                    randomOffsetMin_Pos_X = Math.Clamp(randomOffsetMin_Pos_X, -10000f, 10000f);
                    randomOffsetMin_Pos_Y = Math.Clamp(randomOffsetMin_Pos_Y, -10000f, 10000f);
                    randomOffsetMin_Pos_Z = Math.Clamp(randomOffsetMin_Pos_Z, -10000f, 10000f);

                    randomOffsetMax_Pos_X = Math.Clamp(randomOffsetMax_Pos_X, -10000f, 10000f);
                    randomOffsetMax_Pos_Y = Math.Clamp(randomOffsetMax_Pos_Y, -10000f, 10000f);
                    randomOffsetMax_Pos_Z = Math.Clamp(randomOffsetMax_Pos_Z, -10000f, 10000f);

                    randomOffsetMin_Rot_X = Math.Clamp(randomOffsetMin_Rot_X, 0.0f, 360f);
                    randomOffsetMin_Rot_Y = Math.Clamp(randomOffsetMin_Rot_Y, 0.0f, 360f);
                    randomOffsetMin_Rot_Z = Math.Clamp(randomOffsetMin_Rot_Z, 0.0f, 360f);

                    randomOffsetMax_Rot_X = Math.Clamp(randomOffsetMax_Rot_X, 0.0f, 360f);
                    randomOffsetMax_Rot_Y = Math.Clamp(randomOffsetMax_Rot_Y, 0.0f, 360f);
                    randomOffsetMax_Rot_Z = Math.Clamp(randomOffsetMax_Rot_Z, 0.0f, 360f);

                    randomOffsetMin_Scale_X = Math.Clamp(randomOffsetMin_Scale_X, 0.0f, 100f);
                    randomOffsetMin_Scale_Y = Math.Clamp(randomOffsetMin_Scale_Y, 0.0f, 100f);
                    randomOffsetMin_Scale_Z = Math.Clamp(randomOffsetMin_Scale_Z, 0.0f, 100f);

                    randomOffsetMax_Scale_X = Math.Clamp(randomOffsetMax_Scale_X, 0.0f, 100f);
                    randomOffsetMax_Scale_Y = Math.Clamp(randomOffsetMax_Scale_Y, 0.0f, 100f);
                    randomOffsetMax_Scale_Z = Math.Clamp(randomOffsetMax_Scale_Z, 0.0f, 100f);

                    CFG.Current.Scrambler_OffsetMin_Position_X = randomOffsetMin_Pos_X;
                    CFG.Current.Scrambler_OffsetMin_Position_Y = randomOffsetMin_Pos_Y;
                    CFG.Current.Scrambler_OffsetMin_Position_Z = randomOffsetMin_Pos_Z;

                    CFG.Current.Scrambler_OffsetMax_Position_X = randomOffsetMax_Pos_X;
                    CFG.Current.Scrambler_OffsetMax_Position_Y = randomOffsetMax_Pos_Y;
                    CFG.Current.Scrambler_OffsetMax_Position_Z = randomOffsetMax_Pos_Z;

                    CFG.Current.Scrambler_OffsetMin_Rotation_X = randomOffsetMin_Rot_X;
                    CFG.Current.Scrambler_OffsetMin_Rotation_Y = randomOffsetMin_Rot_Y;
                    CFG.Current.Scrambler_OffsetMin_Rotation_Z = randomOffsetMin_Rot_Z;

                    CFG.Current.Scrambler_OffsetMax_Rotation_X = randomOffsetMax_Rot_X;
                    CFG.Current.Scrambler_OffsetMax_Rotation_Y = randomOffsetMax_Rot_Y;
                    CFG.Current.Scrambler_OffsetMax_Rotation_Z = randomOffsetMax_Rot_Z;

                    CFG.Current.Scrambler_OffsetMin_Scale_X = randomOffsetMin_Scale_X;
                    CFG.Current.Scrambler_OffsetMin_Scale_Y = randomOffsetMin_Scale_Y;
                    CFG.Current.Scrambler_OffsetMin_Scale_Z = randomOffsetMin_Scale_Z;

                    CFG.Current.Scrambler_OffsetMax_Scale_X = randomOffsetMax_Scale_X;
                    CFG.Current.Scrambler_OffsetMax_Scale_Y = randomOffsetMax_Scale_Y;
                    CFG.Current.Scrambler_OffsetMax_Scale_Z = randomOffsetMax_Scale_Z;
                }

                // Replicate
                if (_selectedTool == SelectedTool.Selection_Replicate)
                {
                    ImGui.Text("Replicate the current selection by the following parameters.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {GetKeybindHint(KeyBindings.Current.Toolbar_Replicate.HintText)}");
                    ImGui.Separator();

                    if(ImGui.Checkbox("Line", ref CFG.Current.Replicator_Mode_Line))
                    {
                        CFG.Current.Replicator_Mode_Circle = false;
                        CFG.Current.Replicator_Mode_Square = false;
                        CFG.Current.Replicator_Mode_Sphere = false;
                        CFG.Current.Replicator_Mode_Box = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Replicate the first selection in the Line shape.");
                    }
                    ImGui.SameLine();

                    if(ImGui.Checkbox("Circle", ref CFG.Current.Replicator_Mode_Circle))
                    {
                        CFG.Current.Replicator_Mode_Line = false;
                        CFG.Current.Replicator_Mode_Square = false;
                        CFG.Current.Replicator_Mode_Sphere = false;
                        CFG.Current.Replicator_Mode_Box = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Replicate the first selection in the Circle shape.");
                    }
                    ImGui.SameLine();

                    if(ImGui.Checkbox("Square", ref CFG.Current.Replicator_Mode_Square))
                    {
                        CFG.Current.Replicator_Mode_Circle = false;
                        CFG.Current.Replicator_Mode_Line = false;
                        CFG.Current.Replicator_Mode_Sphere = false;
                        CFG.Current.Replicator_Mode_Box = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Replicate the first selection in the Square shape.");
                    }

                    // WIP
                    /*
                    if (ImGui.Checkbox("Sphere", ref CFG.Current.Replicator_Mode_Sphere))
                    {
                        CFG.Current.Replicator_Mode_Circle = false;
                        CFG.Current.Replicator_Mode_Line = false;
                        CFG.Current.Replicator_Mode_Square = false;
                        CFG.Current.Replicator_Mode_Box = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Replicate the first selection in the Sphere shape.");
                    }
                    ImGui.SameLine();

                    if (ImGui.Checkbox("Box", ref CFG.Current.Replicator_Mode_Box))
                    {
                        CFG.Current.Replicator_Mode_Circle = false;
                        CFG.Current.Replicator_Mode_Line = false;
                        CFG.Current.Replicator_Mode_Square = false;
                        CFG.Current.Replicator_Mode_Sphere = false;
                    }
                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("Replicate the first selection in the Box shape.");
                    }
                    */

                    // Line
                    if (CFG.Current.Replicator_Mode_Line)
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.InputInt("Amount", ref CFG.Current.Replicator_Line_Clone_Amount);
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("The amount of new entities to create (from the first selection).");
                        }

                        ImGui.PushItemWidth(200);
                        ImGui.InputInt("Offset", ref CFG.Current.Replicator_Line_Position_Offset);
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("The distance between each newly created entity.");
                        }

                        if (ImGui.Checkbox("X", ref CFG.Current.Replicator_Line_Position_Offset_Axis_X))
                        {
                            CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                            CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                        }
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("Replicate on the X-axis.");
                        }
                        ImGui.SameLine();
                        if (ImGui.Checkbox("Y", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Y))
                        {
                            CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                            CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                        }
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("Replicate on the Y-axis.");
                        }
                        ImGui.SameLine();
                        if (ImGui.Checkbox("Z", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Z))
                        {
                            CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                            CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                        }
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("Replicate on the Z-axis.");
                        }

                        ImGui.Checkbox("Flip Offset Direction", ref CFG.Current.Replicator_Line_Offset_Direction_Flipped);

                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("When enabled, the position offset will be applied in the opposite direction.");
                        }
                    }

                    // Circle
                    if (CFG.Current.Replicator_Mode_Circle)
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.InputInt("Size", ref CFG.Current.Replicator_Circle_Size);
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("The number of points within the circle on which the entities are placed.");
                        }

                        if (CFG.Current.Replicator_Circle_Size < 1)
                            CFG.Current.Replicator_Circle_Size = 1;

                        if (ImGui.Button("Switch"))
                        {
                            CFG.Current.Replicator_Circle_Radius_Specific_Input = !CFG.Current.Replicator_Circle_Radius_Specific_Input;
                        }
                        ImGui.SameLine();
                        if (CFG.Current.Replicator_Circle_Radius_Specific_Input)
                        {
                            ImGui.PushItemWidth(200);
                            ImGui.InputFloat("Radius", ref CFG.Current.Replicator_Circle_Radius);
                            if (CFG.Current.System_Show_UI_Tooltips)
                            {
                                ImGui.SameLine();
                                Utils.ShowHelpMarker("The radius of the circle on which to place the entities.");
                            }

                        }
                        else
                        {
                            ImGui.PushItemWidth(200);
                            ImGui.SliderFloat("Radius", ref CFG.Current.Replicator_Circle_Radius, 0.1f, 100);
                            if (CFG.Current.System_Show_UI_Tooltips)
                            {
                                ImGui.SameLine();
                                Utils.ShowHelpMarker("The radius of the circle on which to place the entities.");
                            }
                        }
                    }

                    // Square
                    if (CFG.Current.Replicator_Mode_Square)
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.InputInt("Size", ref CFG.Current.Replicator_Square_Size);
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("The number of points on one side of the square on which the entities are placed.");
                        }

                        if (CFG.Current.Replicator_Square_Size < 2)
                            CFG.Current.Replicator_Square_Size = 2;

                        ImGui.PushItemWidth(200);
                        ImGui.InputFloat("Width", ref CFG.Current.Replicator_Square_Width);
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("The width of the square on which to place the entities.");
                        }

                        if (CFG.Current.Replicator_Square_Width < 1)
                            CFG.Current.Replicator_Square_Width = 1;

                        ImGui.PushItemWidth(200);
                        ImGui.InputFloat("Depth", ref CFG.Current.Replicator_Square_Depth);
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("The depth of the square on which to place the entities.");
                        }

                        if (CFG.Current.Replicator_Square_Depth < 1)
                            CFG.Current.Replicator_Square_Depth = 1;
                    }

                    // Sphere
                    if (CFG.Current.Replicator_Mode_Sphere)
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.InputInt("Size", ref CFG.Current.Replicator_Sphere_Size);
                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("The number of points within the sphere on which the entities are placed.");
                        }

                        if (CFG.Current.Replicator_Sphere_Size < 1)
                            CFG.Current.Replicator_Sphere_Size = 1;

                        if (ImGui.Button("Switch"))
                        {
                            CFG.Current.Replicator_Sphere_Horizontal_Radius_Specific_Input = !CFG.Current.Replicator_Sphere_Horizontal_Radius_Specific_Input;
                        }
                        ImGui.SameLine();
                        if (CFG.Current.Replicator_Sphere_Horizontal_Radius_Specific_Input)
                        {
                            ImGui.PushItemWidth(200);
                            ImGui.InputFloat("Horizontal Radius", ref CFG.Current.Replicator_Sphere_Horizontal_Radius);
                            if (CFG.Current.System_Show_UI_Tooltips)
                            {
                                ImGui.SameLine();
                                Utils.ShowHelpMarker("The radius of the sphere on which to place the entities.");
                            }

                        }
                        else
                        {
                            ImGui.PushItemWidth(200);
                            ImGui.SliderFloat("Horizontal Radius", ref CFG.Current.Replicator_Sphere_Horizontal_Radius, 0.1f, 100);
                            if (CFG.Current.System_Show_UI_Tooltips)
                            {
                                ImGui.SameLine();
                                Utils.ShowHelpMarker("The radius of the sphere on which to place the entities.");
                            }
                        }

                        if (ImGui.Button("Switch"))
                        {
                            CFG.Current.Replicator_Sphere_Vertical_Radius_Specific_Input = !CFG.Current.Replicator_Sphere_Vertical_Radius_Specific_Input;
                        }
                        ImGui.SameLine();
                        if (CFG.Current.Replicator_Sphere_Vertical_Radius_Specific_Input)
                        {
                            ImGui.PushItemWidth(200);
                            ImGui.InputFloat("Vertical Radius", ref CFG.Current.Replicator_Sphere_Vertical_Radius);
                            if (CFG.Current.System_Show_UI_Tooltips)
                            {
                                ImGui.SameLine();
                                Utils.ShowHelpMarker("The vertical radius of the sphere on which to place the entities.");
                            }

                        }
                        else
                        {
                            ImGui.PushItemWidth(200);
                            ImGui.SliderFloat("Vertical Radius", ref CFG.Current.Replicator_Sphere_Vertical_Radius, 0.1f, 100);
                            if (CFG.Current.System_Show_UI_Tooltips)
                            {
                                ImGui.SameLine();
                                Utils.ShowHelpMarker("The vertical radius of the sphere on which to place the entities.");
                            }
                        }
                    }

                    // Box

                    ImGui.Separator();

                    ImGui.Checkbox("Apply Scramble Configuration", ref CFG.Current.Replicator_Apply_Scramble_Configuration);

                    if (CFG.Current.System_Show_UI_Tooltips)
                    {
                        ImGui.SameLine();
                        Utils.ShowHelpMarker("When enabled, the Scramble configuration settings will be applied to the newly duplicated entities.");
                    }

                    if (_assetLocator.Type != GameType.DarkSoulsIISOTFS)
                    {
                        ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Replicator_Increment_Entity_ID);

                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("When enabled, the replicated entities will be given new Entity ID. If disabled, the replicated entity ID will be set to 0.");
                        }
                    }

                    if (_assetLocator.Type == GameType.EldenRing)
                    {
                        ImGui.Checkbox("Increment UnkPartNames for Assets", ref CFG.Current.Replicator_Increment_UnkPartNames);

                        if (CFG.Current.System_Show_UI_Tooltips)
                        {
                            ImGui.SameLine();
                            Utils.ShowHelpMarker("When enabled, the duplicated Asset entities UnkPartNames property will be updated.");
                        }
                    }
                }

                ImGui.EndChild();
            }

            ImGui.End();
        }

        public string GetKeybindHint(string hint)
        {
            if (hint == "")
                return "None";
            else
                return hint;
        }

        /// <summary>
        /// Duplicate the current selection
        /// </summary>
        public void DuplicateSelection()
        {
            CloneMapObjectsAction action = new(_universe, _scene,
                    _selection.GetFilteredSelection<MapEntity>().ToList(), true, _assetLocator);
            _actionManager.ExecuteAction(action);
        }

        /// <summary>
        /// Move current selection to the closest grid X, Y, Z
        /// </summary>
        public void MoveSelectionToGrid()
        {
            List<Action> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>(o => o.HasTransform))
            {
                sel.ClearTemporaryTransform(false);
                actlist.Add(sel.GetUpdateTransformAction(GetGridTransform(sel)));
            }

            CompoundAction action = new(actlist);
            _actionManager.ExecuteAction(action);
        }

        /// <summary>
        /// Replicate the current selection.
        /// </summary>
        public void ReplicateSelection()
        {
            ReplicateMapObjectsAction action = new(this, _universe, _scene,
                    _selection.GetFilteredSelection<MapEntity>().ToList(), _assetLocator, _actionManager);
            _actionManager.ExecuteAction(action);
        }

        /// <summary>
        /// Scramble the position, rotation and scale of the current selection.
        /// </summary>
        public void ScambleSelection()
        {
            List<Action> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>(o => o.HasTransform))
            {
                sel.ClearTemporaryTransform(false);
                actlist.Add(sel.GetUpdateTransformAction(GetScrambledTransform(sel)));
            }

            CompoundAction action = new(actlist);
            _actionManager.ExecuteAction(action);
        }

        /// <summary>
        /// Go to the selected object (first if multiple are selected) in the scene tree.
        /// </summary>
        public void GoToInObjectList()
        {
            _selection.GotoTreeTarget = _selection.GetSingleSelection();
        }

        /// <summary>
        /// Frame selected object (first if multiple are selected) in viewport.
        /// </summary>
        public void FrameSelection()
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
                else if (s.Container.RootObject == s)
                {
                    // Selection is transform node
                    Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                    Vector3 pos = s.GetLocalTransform().Position;
                    BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                    if (!first)
                    {
                        first = true;
                        box = nodeBox;
                    }
                    else
                    {
                        box = BoundingBox.Combine(box, nodeBox);
                    }
                }
            }

            if (first)
            {
                _viewport.FrameBox(box);
            }
        }

        /// <summary>
        /// Set visiblity state booleans
        /// </summary>
        public void ForceVisibilityState(bool visible, bool invisible, bool flip)
        {
            CFG.Current.Toolbar_Visibility_State_Enabled = visible;
            CFG.Current.Toolbar_Visibility_State_Disabled = invisible;
            CFG.Current.Toolbar_Visibility_State_Flip = flip;
        }

        /// <summary>
        /// Toggle visiblity of selected objects
        /// </summary>
        public void ToggleEntityVisibility()
        {
            if (CFG.Current.Toolbar_Visibility_Target_Selection)
            {
                HashSet<Entity> selected = _selection.GetFilteredSelection<Entity>();

                foreach (Entity s in selected)
                {
                    if (CFG.Current.Toolbar_Visibility_State_Enabled)
                        s.EditorVisible = true;

                    if (CFG.Current.Toolbar_Visibility_State_Disabled)
                        s.EditorVisible = false;

                    if (CFG.Current.Toolbar_Visibility_State_Flip)
                        s.EditorVisible = !s.EditorVisible;
                }
            }
            if(CFG.Current.Toolbar_Visibility_Target_All)
            {
                foreach (ObjectContainer m in _universe.LoadedObjectContainers.Values)
                {
                    if (m == null)
                    {
                        continue;
                    }

                    foreach (Entity obj in m.Objects)
                    {
                        if (CFG.Current.Toolbar_Visibility_State_Enabled)
                            obj.EditorVisible = true;

                        if (CFG.Current.Toolbar_Visibility_State_Disabled)
                            obj.EditorVisible = false;

                        if (CFG.Current.Toolbar_Visibility_State_Flip)
                            obj.EditorVisible = !obj.EditorVisible;
                    }
                }
            }
        }

        /// <summary>
        /// Move current selection to the current camera position
        /// </summary>
        public void MoveSelectionToCamera()
        {
            List<Action> actlist = new();
            HashSet<Entity> sels = _selection.GetFilteredSelection<Entity>(o => o.HasTransform);

            Vector3 camDir = Vector3.Transform(Vector3.UnitZ, _viewport.WorldView.CameraTransform.RotationMatrix);
            Vector3 camPos = _viewport.WorldView.CameraTransform.Position;
            Vector3 targetCamPos = camPos + (camDir * CFG.Current.Toolbar_Move_to_Camera_Offset);

            // Get the accumulated center position of all selections
            Vector3 accumPos = Vector3.Zero;
            foreach (Entity sel in sels)
            {
                if (Gizmos.Origin == Gizmos.GizmosOrigin.BoundingBox && sel.RenderSceneMesh != null)
                {
                    // Use bounding box origin as center
                    accumPos += sel.RenderSceneMesh.GetBounds().GetCenter();
                }
                else
                {
                    // Use actual position as center
                    accumPos += sel.GetRootLocalTransform().Position;
                }
            }

            Transform centerT = new(accumPos / sels.Count, Vector3.Zero);

            // Offset selection positions to place accumulated center in front of camera
            foreach (Entity sel in sels)
            {
                Transform localT = sel.GetLocalTransform();
                Transform rootT = sel.GetRootTransform();

                // Get new localized position by applying reversed root offsets to target camera position.  
                Vector3 newPos = Vector3.Transform(targetCamPos, Quaternion.Inverse(rootT.Rotation))
                                 - Vector3.Transform(rootT.Position, Quaternion.Inverse(rootT.Rotation));

                // Offset from center of multiple selections.
                Vector3 localCenter = Vector3.Transform(centerT.Position, Quaternion.Inverse(rootT.Rotation))
                                          - Vector3.Transform(rootT.Position, Quaternion.Inverse(rootT.Rotation));
                Vector3 offsetFromCenter = localCenter - localT.Position;
                newPos -= offsetFromCenter;

                Transform newT = new(newPos, localT.EulerRotation);

                actlist.Add(sel.GetUpdateTransformAction(newT));
            }

            if (actlist.Any())
            {
                CompoundAction action = new(actlist);
                _actionManager.ExecuteAction(action);
            }
        }

        /// <summary>
        /// Rotate the selected objects by a fixed amount on the specified axis
        /// </summary>
        public void ArbitraryRotation_Selection(Vector3 axis, bool pivot)
        {
            List<Action> actlist = new();
            HashSet<Entity> sels = _selection.GetFilteredSelection<Entity>(o => o.HasTransform);

            // Get the center position of the selections
            Vector3 accumPos = Vector3.Zero;
            foreach (Entity sel in sels)
            {
                accumPos += sel.GetLocalTransform().Position;
            }

            Transform centerT = new(accumPos / sels.Count, Vector3.Zero);

            foreach (Entity s in sels)
            {
                Transform objT = s.GetLocalTransform();

                var radianRotateAmount = 0.0f;
                var rot_x = objT.EulerRotation.X;
                var rot_y = objT.EulerRotation.Y;
                var rot_z = objT.EulerRotation.Z;

                var newPos = Transform.Default;

                if (axis.X != 0)
                {
                    radianRotateAmount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment;
                    rot_x = objT.EulerRotation.X + radianRotateAmount;
                }

                if (axis.Y != 0)
                {
                    radianRotateAmount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment;
                    rot_y = objT.EulerRotation.Y + radianRotateAmount;
                }

                if (pivot)
                {
                    newPos = Utils.RotateVectorAboutPoint(objT.Position, centerT.Position, axis, radianRotateAmount);
                }
                else
                {
                    newPos.Position = objT.Position;
                }

                newPos.EulerRotation = new Vector3(rot_x, rot_y, rot_z);

                actlist.Add(s.GetUpdateTransformAction(newPos));
            }

            if (actlist.Any())
            {
                CompoundAction action = new(actlist);
                _actionManager.ExecuteAction(action);
            }
        }

        /// <summary>
        /// Set current selection to fixed rotation.
        /// </summary>
        public void SetSelectionToFixedRotation()
        {
            List<Action> actlist = new();

            HashSet<Entity> selected = _selection.GetFilteredSelection<Entity>(o => o.HasTransform);
            foreach (Entity s in selected)
            {
                Vector3 pos = s.GetLocalTransform().Position;
                Transform newRot = new(pos, CFG.Current.Toolbar_Rotate_FixedAngle);

                actlist.Add(s.GetUpdateTransformAction(newRot));
            }

            if (actlist.Any())
            {
                CompoundAction action = new(actlist);
                _actionManager.ExecuteAction(action);
            }
        }

        private double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public void DummySelection()
        {
            string[] sourceTypes = { "Enemy", "Object", "Asset" };
            string[] targetTypes = { "DummyEnemy", "DummyObject", "DummyAsset" };
            DummyUndummySelection(sourceTypes, targetTypes);
        }

        public void UnDummySelection()
        {
            string[] sourceTypes = { "DummyEnemy", "DummyObject", "DummyAsset" };
            string[] targetTypes = { "Enemy", "Object", "Asset" };
            DummyUndummySelection(sourceTypes, targetTypes);
        }

        private void DummyUndummySelection(string[] sourceTypes, string[] targetTypes)
        {
            Type msbclass;
            switch (_assetLocator.Type)
            {
                case GameType.DemonsSouls:
                    msbclass = typeof(MSBD);
                    break;
                case GameType.DarkSoulsPTDE:
                case GameType.DarkSoulsRemastered:
                    msbclass = typeof(MSB1);
                    break;
                case GameType.DarkSoulsIISOTFS:
                    msbclass = typeof(MSB2);
                    //break;
                    return; //idk how ds2 dummies should work
                case GameType.DarkSoulsIII:
                    msbclass = typeof(MSB3);
                    break;
                case GameType.Bloodborne:
                    msbclass = typeof(MSBB);
                    break;
                case GameType.Sekiro:
                    msbclass = typeof(MSBS);
                    break;
                case GameType.EldenRing:
                    msbclass = typeof(MSBE);
                    break;
                case GameType.ArmoredCoreVI:
                    msbclass = typeof(MSB_AC6);
                    break;
                default:
                    throw new ArgumentException("type must be valid");
            }

            List<MapEntity> sourceList = _selection.GetFilteredSelection<MapEntity>().ToList();

            ChangeMapObjectType action = new(_universe, msbclass, sourceList, sourceTypes, targetTypes, "Part", true);
            _actionManager.ExecuteAction(action);
        }
        private Transform GetGridTransform(Entity sel)
        {
            Transform objT = sel.GetLocalTransform();

            var newTransform = Transform.Default;
            var newPos = objT.Position;
            var newRot = objT.Rotation;
            var newScale = objT.Scale;

            if (CFG.Current.Toolbar_Move_to_Grid_X)
            {
                float temp = newPos[0] / CFG.Current.Viewport_Grid_Square_Size;
                float newPosX = (float)Math.Round(temp, 0) * CFG.Current.Viewport_Grid_Square_Size;

                newPos = new Vector3(newPosX, newPos[1], newPos[2]);
            }

            if (CFG.Current.Toolbar_Move_to_Grid_Z)
            {
                float temp = newPos[2] / CFG.Current.Viewport_Grid_Square_Size;
                float newPosZ = (float)Math.Round(temp, 0) * CFG.Current.Viewport_Grid_Square_Size;

                newPos = new Vector3(newPos[0], newPos[1], newPosZ);
            }

            if (CFG.Current.Toolbar_Move_to_Grid_Y)
            {
                newPos = new Vector3(newPos[0], CFG.Current.Viewport_Grid_Height, newPos[2]);
            }

            newTransform.Position = newPos;
            newTransform.Rotation = newRot;
            newTransform.Scale = newScale;

            return newTransform;
        }

        public Transform GetScrambledTransform(Entity sel)
        {
            float posOffset_X = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Position_X, CFG.Current.Scrambler_OffsetMax_Position_X);
            float posOffset_Y = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Position_Y, CFG.Current.Scrambler_OffsetMax_Position_Y);
            float posOffset_Z = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Position_Z, CFG.Current.Scrambler_OffsetMax_Position_Z);

            float rotOffset_X = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Rotation_X, CFG.Current.Scrambler_OffsetMax_Rotation_X);
            float rotOffset_Y = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Rotation_Y, CFG.Current.Scrambler_OffsetMax_Rotation_Y);
            float rotOffset_Z = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Rotation_Z, CFG.Current.Scrambler_OffsetMax_Rotation_Z);

            float scaleOffset_X = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Scale_X, CFG.Current.Scrambler_OffsetMax_Scale_X);
            float scaleOffset_Y = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Scale_Y, CFG.Current.Scrambler_OffsetMax_Scale_Y);
            float scaleOffset_Z = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Scale_Z, CFG.Current.Scrambler_OffsetMax_Scale_Z);

            Transform objT = sel.GetLocalTransform();

            var newTransform = Transform.Default;

            var radianRotateAmount = 0.0f;
            var rot_x = objT.EulerRotation.X;
            var rot_y = objT.EulerRotation.Y;
            var rot_z = objT.EulerRotation.Z;

            var newPos = objT.Position;
            var newRot = objT.Rotation;
            var newScale = objT.Scale;

            if (CFG.Current.Scrambler_RandomisePosition_X)
            {
                newPos = new Vector3(newPos[0] + posOffset_X, newPos[1], newPos[2]);
            }
            if (CFG.Current.Scrambler_RandomisePosition_Y)
            {
                newPos = new Vector3(newPos[0], newPos[1] + posOffset_Y, newPos[2]);
            }
            if (CFG.Current.Scrambler_RandomisePosition_Z)
            {
                newPos = new Vector3(newPos[0], newPos[1], newPos[2] + posOffset_Z);
            }

            newTransform.Position = newPos;

            if (CFG.Current.Scrambler_RandomiseRotation_X)
            {
                radianRotateAmount = (float)Math.PI / 180 * rotOffset_X;
                rot_x = objT.EulerRotation.X + radianRotateAmount;
            }
            if (CFG.Current.Scrambler_RandomiseRotation_Y)
            {
                radianRotateAmount = (float)Math.PI / 180 * rotOffset_Y;
                rot_y = objT.EulerRotation.Y + radianRotateAmount;
            }
            if (CFG.Current.Scrambler_RandomiseRotation_Z)
            {
                radianRotateAmount = (float)Math.PI / 180 * rotOffset_Z;
                rot_z = objT.EulerRotation.Z + radianRotateAmount;
            }

            if (CFG.Current.Scrambler_RandomiseRotation_X || CFG.Current.Scrambler_RandomiseRotation_Y || CFG.Current.Scrambler_RandomiseRotation_Z)
            {
                newTransform.EulerRotation = new Vector3(rot_x, rot_y, rot_z);
            }
            else
            {
                newTransform.Rotation = newRot;
            }

            // If shared scale, the scale randomisation will be the same for X, Y, Z
            if (CFG.Current.Scrambler_RandomiseScale_SharedScale)
            {
                scaleOffset_Y = scaleOffset_X;
                scaleOffset_Z = scaleOffset_X;
            }

            if (CFG.Current.Scrambler_RandomiseScale_X)
            {
                newScale = new Vector3(scaleOffset_X, newScale[1], newScale[2]);
            }
            if (CFG.Current.Scrambler_RandomiseScale_Y)
            {
                newScale = new Vector3(newScale[0], scaleOffset_Y, newScale[2]);
            }
            if (CFG.Current.Scrambler_RandomiseScale_Z)
            {
                newScale = new Vector3(newScale[0], newScale[1], scaleOffset_Z);
            }

            newTransform.Scale = newScale;

            return newTransform;
        }
    }
}

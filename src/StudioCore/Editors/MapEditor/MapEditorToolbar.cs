using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Microsoft.Toolkit.HighPerformance;
using StudioCore.Gui;
using StudioCore.Scene;
using StudioCore.Utilities;
using System.IO;
using System;
using DotNext;
using Veldrid.Utilities;
using SoulsFormats;
using StudioCore.Interface;
using System.Reflection;
using StudioCore.UserProject;
using StudioCore.Banks;
using StudioCore.MsbEditor;

namespace StudioCore.Editors.MapEditor
{
    public enum SelectedTool
    {
        None,
        // Global
        Selection_Duplicate_Entity_ID,
        Selection_Render_Patrol_Routes,
        Selection_Generate_Navigation_Data,
        Selection_Toggle_Object_Visibility_by_Tag,

        // Selection
        Selection_Toggle_Visibility,
        Selection_Go_to_in_Object_List,
        Selection_Move_to_Camera,
        Selection_Frame_in_Viewport,
        Selection_Create,
        Selection_Duplicate,
        Selection_Rotate,
        Selection_Toggle_Presence,
        Selection_Move_to_Grid,
        Selection_Scramble,
        Selection_Replicate
    }

    public class MapEditorToolbar
    {
        private readonly ViewportActionManager _actionManager;

        private readonly RenderScene _scene;
        private readonly ViewportSelection _selection;

        private Universe _universe;

        private IViewport _viewport;

        private SelectedTool _selectedTool;

        private IEnumerable<ObjectContainer> _loadedMaps;
        private int _createEntityMapIndex;

        private List<(string, Type)> _eventClasses = new();
        private List<(string, Type)> _partsClasses = new();
        private List<(string, Type)> _regionClasses = new();

        private Type _createPartSelectedType;
        private Type _createRegionSelectedType;
        private Type _createEventSelectedType;

        private List<string> entityIdentifiers = new List<string>();

        private bool NavigationDataProcessed = false;

        private int FrameCount = 0;

        public MapEditorToolbar(RenderScene scene, ViewportSelection sel, ViewportActionManager manager, Universe universe, IViewport viewport)
        {
            _scene = scene;
            _selection = sel;
            _actionManager = manager;
            _universe = universe;

            _viewport = viewport;
        }

        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            // This is to reset temporary Text elements. Only used by Generate Navigation Data currently.
            if (FrameCount > 1000)
            {
                FrameCount = 0;
                NavigationDataProcessed = false;
            }
            FrameCount++;

            if (Project.Type == ProjectType.Undefined)
                return;

            _loadedMaps = _universe.LoadedObjectContainers.Values.Where(x => x != null);

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Toolbar##MsbMenubar"))
            {
                ImGui.Columns(2);

                // Selection List
                ImGui.BeginChild("toolselection");

                ImGui.Separator();
                ImGui.Text("Selection actions");
                ImguiUtils.ShowHelpMarker("Double-click to use. These actions are done in the context of a selection.");
                ImGui.Separator();

                // Go to in Object List
                if (CFG.Current.Toolbar_Show_Go_to_in_Object_List)
                {
                    if (ImGui.Selectable("Go to in Object List##tool_Selection_GoToInObjectList", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedTool = SelectedTool.Selection_Go_to_in_Object_List;

                        if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                        {
                            GoToInObjectList();
                        }
                    }
                }

                // Move to Camera
                if (CFG.Current.Toolbar_Show_Move_to_Camera)
                {
                    if (ImGui.Selectable("Move to Camera##tool_Selection_MoveToCamera", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedTool = SelectedTool.Selection_Move_to_Camera;

                        if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                        {
                            MoveSelectionToCamera();
                        }
                    }
                }

                // Frame in Viewport
                if (CFG.Current.Toolbar_Show_Frame_in_Viewport)
                {
                    if (ImGui.Selectable("Frame in Viewport##tool_Selection_FrameInViewport", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedTool = SelectedTool.Selection_Frame_in_Viewport;

                        if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                        {
                            FrameSelection();
                        }
                    }
                }

                // Toggle Visibility
                if (CFG.Current.Toolbar_Show_Toggle_Visibility)
                {
                    if (ImGui.Selectable("Toggle Visibility##tool_Selection_ToggleVisibility", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedTool = SelectedTool.Selection_Toggle_Visibility;

                        if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                        {
                            ToggleEntityVisibility();
                        }
                    }
                }

                // Create
                if (CFG.Current.Toolbar_Show_Create)
                {
                    if (ImGui.Selectable("Create##tool_Selection_Create", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedTool = SelectedTool.Selection_Create;

                        if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                        {
                            CreateNewMapObject();
                        }
                    }
                }

                // Duplicate
                if (CFG.Current.Toolbar_Show_Duplicate)
                {
                    if (ImGui.Selectable("Duplicate##tool_Selection_Duplicate", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedTool = SelectedTool.Selection_Duplicate;

                        if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                        {
                            DuplicateSelection();
                        }
                    }
                }

                // Rotate
                if (CFG.Current.Toolbar_Show_Rotate)
                {
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
                }

                // Toggle Presence
                if (CFG.Current.Toolbar_Show_Toggle_Presence)
                {
                    if (ImGui.Selectable("Toggle Presence##tool_Selection_Presence", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedTool = SelectedTool.Selection_Toggle_Presence;

                        if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                        {
                            if (CFG.Current.Toolbar_Presence_Dummy_Type_ER)
                            {
                                if (CFG.Current.Toolbar_Presence_Dummify)
                                {
                                    ER_DummySelection();
                                }
                                if (CFG.Current.Toolbar_Presence_Undummify)
                                {
                                    ER_UnDummySelection();
                                }
                            }
                            else
                            {
                                if (CFG.Current.Toolbar_Presence_Dummify)
                                {
                                    DummySelection();
                                }
                                if (CFG.Current.Toolbar_Presence_Undummify)
                                {
                                    UnDummySelection();
                                }
                            }
                        }
                    }
                }

                // Scramble
                if (CFG.Current.Toolbar_Show_Scramble)
                {
                    if (ImGui.Selectable("Scramble##tool_Selection_Scramble", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedTool = SelectedTool.Selection_Scramble;

                        if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                        {
                            ScambleSelection();
                        }
                    }
                }

                // Replicate
                if (CFG.Current.Toolbar_Show_Replicate)
                {
                    if (ImGui.Selectable("Replicate##tool_Selection_Replicate", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedTool = SelectedTool.Selection_Replicate;

                        if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                        {
                            ReplicateSelection();
                        }
                    }
                }

                // Move to Grid
                if (CFG.Current.Toolbar_Show_Move_to_Grid)
                {
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
                }

                ImGui.Separator();
                ImGui.Text("Global actions");
                ImguiUtils.ShowHelpMarker("Double-click to use. These actions are done in the global context.");
                ImGui.Separator();

                // Entity ID
                if (CFG.Current.Toolbar_Show_Check_Duplicate_Entity_ID)
                {
                    if (ImGui.Selectable("Check Duplicate Entity ID##tool_Selection_Duplicate_Entity_ID", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedTool = SelectedTool.Selection_Duplicate_Entity_ID;

                        if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                        {
                            if (_loadedMaps.Any())
                            {
                                CheckDuplicateEntityIdentifier();
                            }
                        }
                    }
                }

                // Patrol Routes
                if (CFG.Current.Toolbar_Show_Render_Patrol_Routes)
                {
                    if (Project.Type is not ProjectType.DS2S)
                    {
                        if (ImGui.Selectable("Patrol Routes##tool_Selection_Render_Patrol_Routes", false, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            _selectedTool = SelectedTool.Selection_Render_Patrol_Routes;

                            if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                            {
                                RenderPatrolRoutes();
                            }
                        }
                    }
                }

                // Generate Navigation Data
                if (CFG.Current.Toolbar_Show_Navigation_Data)
                {
                    if (Project.Type is ProjectType.DES || Project.Type is ProjectType.DS1 || Project.Type is ProjectType.DS1R)
                    {
                        if (ImGui.Selectable("Navigation Data##tool_Selection_Generate_Navigation_Data", false, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            _selectedTool = SelectedTool.Selection_Generate_Navigation_Data;

                            if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                            {
                                GenerateNavigationData();
                            }
                        }
                    }
                }

                // Toggle Object Visibility by Tag
                if (CFG.Current.Toolbar_Show_Toggle_Object_Visibility_by_Tag)
                {
                    if (ImGui.Selectable("Toggle Object Visibility by Tag##tool_Selection_Toggle_Object_Visibility_by_Tag", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedTool = SelectedTool.Selection_Toggle_Object_Visibility_by_Tag;

                        if (ImGui.IsMouseDoubleClicked(0))
                        {
                            ToggleObjectVisibilityByTag();
                        }
                    }
                }


                ImGui.EndChild();

                // Configuration Window
                ImGui.NextColumn();

                ImGui.BeginChild("toolconfiguration");

                // Create
                if (_selectedTool == SelectedTool.Selection_Create)
                {
                    ImGui.Text("Create a new object within the target map.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Create.HintText)}");
                    ImGui.Separator();

                    if (!_loadedMaps.Any())
                    {
                        ImGui.Text("No maps have been loaded yet.");
                    }
                    else
                    {
                        var map = (Map)_loadedMaps.ElementAt(_createEntityMapIndex);

                        ImGui.Combo("Target Map", ref _createEntityMapIndex, _loadedMaps.Select(e => e.Name).ToArray(), _loadedMaps.Count());

                        if (map.BTLParents.Any())
                        {
                            if (ImGui.Checkbox("BTL Light", ref CFG.Current.Toolbar_Create_Light))
                            {
                                CFG.Current.Toolbar_Create_Part = false;
                                CFG.Current.Toolbar_Create_Region = false;
                                CFG.Current.Toolbar_Create_Event = false;
                            }
                            ImguiUtils.ShowHelpMarker("Create a BTL Light object.");
                        }

                        if (ImGui.Checkbox("Part", ref CFG.Current.Toolbar_Create_Part))
                        {
                            CFG.Current.Toolbar_Create_Light = false;
                            CFG.Current.Toolbar_Create_Region = false;
                            CFG.Current.Toolbar_Create_Event = false;
                        }
                        ImguiUtils.ShowHelpMarker("Create a Part object.");

                        if (ImGui.Checkbox("Region", ref CFG.Current.Toolbar_Create_Region))
                        {
                            CFG.Current.Toolbar_Create_Light = false;
                            CFG.Current.Toolbar_Create_Part = false;
                            CFG.Current.Toolbar_Create_Event = false;
                        }
                        ImguiUtils.ShowHelpMarker("Create a Region object.");

                        if (ImGui.Checkbox("Event", ref CFG.Current.Toolbar_Create_Event))
                        {
                            CFG.Current.Toolbar_Create_Light = false;
                            CFG.Current.Toolbar_Create_Region = false;
                            CFG.Current.Toolbar_Create_Part = false;
                        }
                        ImguiUtils.ShowHelpMarker("Create an Event object.");

                        ImGui.Separator();

                        if (CFG.Current.Toolbar_Create_Light)
                        {
                            // Nothing
                        }

                        if (CFG.Current.Toolbar_Create_Part)
                        {
                            ImGui.Text("Part Type:");
                            ImGui.Separator();
                            ImGui.BeginChild("msb_part_selection");

                            foreach ((string, Type) p in _partsClasses)
                            {
                                if (ImGui.Selectable(p.Item1, p.Item2 == _createPartSelectedType))
                                {
                                    _createPartSelectedType = p.Item2;
                                }
                            }

                            ImGui.EndChild();
                        }

                        if (CFG.Current.Toolbar_Create_Region)
                        {
                            // MSB format that only have 1 region type
                            if (_regionClasses.Count == 1)
                            {
                                _createRegionSelectedType = _regionClasses[0].Item2;
                            }
                            else
                            {
                                ImGui.Text("Region Type:");
                                ImGui.Separator();
                                ImGui.BeginChild("msb_region_selection");

                                foreach ((string, Type) p in _regionClasses)
                                {
                                    if (ImGui.Selectable(p.Item1, p.Item2 == _createRegionSelectedType))
                                    {
                                        _createRegionSelectedType = p.Item2;
                                    }
                                }

                                ImGui.EndChild();
                            }
                        }

                        if (CFG.Current.Toolbar_Create_Event)
                        {
                            ImGui.Text("Event Type:");
                            ImGui.Separator();
                            ImGui.BeginChild("msb_event_selection");

                            foreach ((string, Type) p in _eventClasses)
                            {
                                if (ImGui.Selectable(p.Item1, p.Item2 == _createEventSelectedType))
                                {
                                    _createEventSelectedType = p.Item2;
                                }
                            }

                            ImGui.EndChild();
                        }

                        ImGui.Separator();
                    }
                }

                // Render Patrol Routes
                if (_selectedTool == SelectedTool.Selection_Render_Patrol_Routes)
                {
                    ImGui.Text("Toggle the rendering of patrol route connections.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_RenderEnemyPatrolRoutes.HintText)}");
                    ImGui.Separator();

                    if (ImGui.Button("Clear"))
                    {
                        PatrolDrawManager.Clear();
                    }
                }

                // Generate Navigation Data
                if (_selectedTool == SelectedTool.Selection_Generate_Navigation_Data)
                {
                    ImGui.Text("Regenerate the navigation data files used for pathfinding.");
                    ImGui.Separator();

                    if (NavigationDataProcessed)
                    {
                        ImGui.Text("Navigation data has been regenerated for all maps.");
                    }
                }

                // Toggle Object Visibility by tag
                if (_selectedTool == SelectedTool.Selection_Toggle_Object_Visibility_by_Tag)
                {
                    ImGui.Text("Toggle the visibility of map objects, filtering the targets by tag\n(you can view tags in the Model or Map Asset Browsers).");
                    ImGui.Separator();

                    ImGui.Text("Target Tag:");
                    ImGui.InputText("##targetTag", ref CFG.Current.Toolbar_Tag_Visibility_Target, 255);
                    ImguiUtils.ShowHelpMarker("Specific which tag the map objects will be filtered by.");

                    ImGui.Text("State:");
                    if (ImGui.Checkbox("Visible", ref CFG.Current.Toolbar_Tag_Visibility_State_Enabled))
                    {
                        CFG.Current.Toolbar_Tag_Visibility_State_Disabled = false;
                    }
                    ImguiUtils.ShowHelpMarker("Set the visible state to enabled.");

                    if (ImGui.Checkbox("Invisible", ref CFG.Current.Toolbar_Tag_Visibility_State_Disabled))
                    {
                        CFG.Current.Toolbar_Tag_Visibility_State_Enabled = false;
                    }
                    ImguiUtils.ShowHelpMarker("Set the visible state to disabled.");
                }

                // Go to in Object List
                if (_selectedTool == SelectedTool.Selection_Go_to_in_Object_List)
                {
                    ImGui.Text("Move the camera to the current selection (first if multiple are selected).");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Go_to_Selection_in_Object_List.HintText)}");
                    ImGui.Separator();
                }

                // Move to Camera
                if (_selectedTool == SelectedTool.Selection_Move_to_Camera)
                {
                    ImGui.Text("Move the current selection to the camera position.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Move_Selection_to_Camera.HintText)}");
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
                        ImguiUtils.ShowHelpMarker("Set the distance at which the current selection is offset from the camera when this action is used.");

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
                        ImguiUtils.ShowHelpMarker("Set the distance at which the current selection is offset from the camera when this action is used.");
                    }
                }

                // Frame in Viewport
                if (_selectedTool == SelectedTool.Selection_Frame_in_Viewport)
                {
                    ImGui.Text("Frame the current selection in the viewport (first if multiple are selected).");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Frame_Selection_in_Viewport.HintText)}");
                    ImGui.Separator();
                }

                // Toggle Visibility
                if (_selectedTool == SelectedTool.Selection_Toggle_Visibility)
                {
                    ImGui.Text("Toggle the visibility of the current selection or all objects.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Flip.HintText)} for Selection (Flip).");
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Flip.HintText)} for all Objects (Flip).");

                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Enabled.HintText)} for Selection (Enabled).");
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Enabled.HintText)} for all Objects (Enabled).");

                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Disabled.HintText)} for Selection (Disabled).");
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Disabled.HintText)} for all Objects (Disabled).");

                    ImGui.Separator();

                    ImGui.Text("Target:");
                    if (ImGui.Checkbox("Selection", ref CFG.Current.Toolbar_Visibility_Target_Selection))
                    {
                        CFG.Current.Toolbar_Visibility_Target_All = false;
                    }
                    ImguiUtils.ShowHelpMarker("Set the target state to our current selection.");

                    if (ImGui.Checkbox("All", ref CFG.Current.Toolbar_Visibility_Target_All))
                    {
                        CFG.Current.Toolbar_Visibility_Target_Selection = false;
                    }
                    ImguiUtils.ShowHelpMarker("Set the target state to all objects.");

                    ImGui.Separator();
                    ImGui.Text("State:");
                    if (ImGui.Checkbox("Visible", ref CFG.Current.Toolbar_Visibility_State_Enabled))
                    {
                        CFG.Current.Toolbar_Visibility_State_Disabled = false;
                        CFG.Current.Toolbar_Visibility_State_Flip = false;
                    }
                    ImguiUtils.ShowHelpMarker("Set the target selection visible state to enabled.");

                    if (ImGui.Checkbox("Invisible", ref CFG.Current.Toolbar_Visibility_State_Disabled))
                    {
                        CFG.Current.Toolbar_Visibility_State_Enabled = false;
                        CFG.Current.Toolbar_Visibility_State_Flip = false;
                    }
                    ImguiUtils.ShowHelpMarker("Set the target selection visible state to disabled.");

                    if (ImGui.Checkbox("Flip", ref CFG.Current.Toolbar_Visibility_State_Flip))
                    {
                        CFG.Current.Toolbar_Visibility_State_Enabled = false;
                        CFG.Current.Toolbar_Visibility_State_Disabled = false;
                    }
                    ImguiUtils.ShowHelpMarker("Set the target selection visible state to opposite of its current state.");

                }

                // Duplicate
                if (_selectedTool == SelectedTool.Selection_Duplicate)
                {
                    ImGui.Text("Duplicate the current selection.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Core_Duplicate.HintText)}");
                    ImGui.Separator();

                    if (Project.Type != ProjectType.DS2S && Project.Type != ProjectType.AC6)
                    {
                        ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Toolbar_Duplicate_Increment_Entity_ID);
                        ImguiUtils.ShowHelpMarker("When enabled, the duplicated entities will be given a new valid Entity ID.");
                    }

                    if (Project.Type == ProjectType.ER)
                    {
                        ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Toolbar_Duplicate_Increment_InstanceID);
                        ImguiUtils.ShowHelpMarker("When enabled, the duplicated entities will be given a new valid Instance ID.");
                    }

                    if (Project.Type == ProjectType.ER)
                    {
                        ImGui.Checkbox("Increment UnkPartNames for Assets", ref CFG.Current.Toolbar_Duplicate_Increment_UnkPartNames);
                        ImguiUtils.ShowHelpMarker("When enabled, the duplicated Asset entities UnkPartNames property will be updated.");
                    }
                }

                // Rotate
                if (_selectedTool == SelectedTool.Selection_Rotate)
                {
                    ImGui.Text("Rotate the current selection by the following parameters.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Rotate_X.HintText)} for Rotate X");
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Rotate_Y.HintText)} for Rotate Y");
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Rotate_Y_Pivot.HintText)} for Rotate Pivot Y");
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Reset_Rotation.HintText)} for Fixed Rotation");
                    ImGui.Separator();

                    var rot = CFG.Current.Toolbar_Rotate_Increment;

                    if (ImGui.Checkbox("X", ref CFG.Current.Toolbar_Rotate_X))
                    {
                        CFG.Current.Toolbar_Rotate_Y = false;
                        CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                        CFG.Current.Toolbar_Fixed_Rotate = false;
                    }
                    ImguiUtils.ShowHelpMarker("Set the rotation axis to X.");

                    ImGui.SameLine();
                    if (ImGui.Checkbox("Y", ref CFG.Current.Toolbar_Rotate_Y))
                    {
                        CFG.Current.Toolbar_Rotate_X = false;
                        CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                        CFG.Current.Toolbar_Fixed_Rotate = false;
                    }
                    ImguiUtils.ShowHelpMarker("Set the rotation axis to Y.");

                    ImGui.SameLine();
                    if (ImGui.Checkbox("Y Pivot", ref CFG.Current.Toolbar_Rotate_Y_Pivot))
                    {
                        CFG.Current.Toolbar_Rotate_Y = false;
                        CFG.Current.Toolbar_Rotate_X = false;
                        CFG.Current.Toolbar_Fixed_Rotate = false;
                    }
                    ImguiUtils.ShowHelpMarker("Set the rotation axis to Y and pivot with respect to others within the selection.");

                    ImGui.SameLine();
                    if (ImGui.Checkbox("Fixed Rotation", ref CFG.Current.Toolbar_Fixed_Rotate))
                    {
                        CFG.Current.Toolbar_Rotate_Y = false;
                        CFG.Current.Toolbar_Rotate_X = false;
                        CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                    }
                    ImguiUtils.ShowHelpMarker("Set the rotation axis to specified values below.");

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
                    ImguiUtils.ShowHelpMarker("Set the angle increment amount used by the rotation.");

                    var x = CFG.Current.Toolbar_Rotate_FixedAngle[0];
                    var y = CFG.Current.Toolbar_Rotate_FixedAngle[1];
                    var z = CFG.Current.Toolbar_Rotate_FixedAngle[2];

                    ImGui.Text("Fixed Rotation");
                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("X##fixedRotationX", ref x))
                    {
                        x = Math.Clamp(x, -360f, 360f);
                    }
                    ImguiUtils.ShowHelpMarker("Set the X component of the fixed rotation action.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("Y##fixedRotationX", ref y))
                    {
                        y = Math.Clamp(y, -360f, 360f);
                    }
                    ImguiUtils.ShowHelpMarker("Set the Y component of the fixed rotation action.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("Z##fixedRotationZ", ref z))
                    {
                        z = Math.Clamp(z, -360f, 360f);
                    }
                    ImguiUtils.ShowHelpMarker("Set the Z component of the fixed rotation action.");

                    ImGui.SameLine();

                    CFG.Current.Toolbar_Rotate_FixedAngle = new Vector3(x, y, z);
                }

                // Toggle Presence
                if (_selectedTool == SelectedTool.Selection_Toggle_Presence)
                {
                    if (CFG.Current.Toolbar_Presence_Dummy_Type_ER)
                        ImGui.Text("Toggle the load status of the current selection.");
                    else
                        ImGui.Text("Toggle the Dummy status of the current selection.");

                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Dummify.HintText)} for Disable");
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Undummify.HintText)} for Enable");
                    ImGui.Separator();

                    if (ImGui.Checkbox("Disable", ref CFG.Current.Toolbar_Presence_Dummify))
                    {
                        CFG.Current.Toolbar_Presence_Undummify = false;
                    }
                    if (CFG.Current.Toolbar_Presence_Dummy_Type_ER)
                        ImguiUtils.ShowHelpMarker("Make the current selection Dummy Objects/Asset/Enemy types.");
                    else
                        ImguiUtils.ShowHelpMarker("Disable the current selection, preventing them from being loaded in-game.");

                    if (ImGui.Checkbox("Enable", ref CFG.Current.Toolbar_Presence_Undummify))
                    {
                        CFG.Current.Toolbar_Presence_Dummify = false;
                    }
                    if (CFG.Current.Toolbar_Presence_Dummy_Type_ER)
                        ImguiUtils.ShowHelpMarker("Make the current selection (if Dummy) normal Objects/Asset/Enemy types.");
                    else
                        ImguiUtils.ShowHelpMarker("Enable the current selection, allow them to be loaded in-game.");

                    if (Project.Type == ProjectType.ER)
                    {
                        ImGui.Checkbox("Use Game Edition Disable", ref CFG.Current.Toolbar_Presence_Dummy_Type_ER);
                        ImguiUtils.ShowHelpMarker("Use the GameEditionDisable property to disable entities instead of the Dummy entity system.");
                    }
                }

                // Move to Grid
                if (_selectedTool == SelectedTool.Selection_Move_to_Grid)
                {
                    ImGui.Text("Set the current selection to the closest grid position.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Set_to_Grid.HintText)}");
                    ImGui.Separator();

                    ImGui.Checkbox("X", ref CFG.Current.Toolbar_Move_to_Grid_X);
                    ImguiUtils.ShowHelpMarker("Move the current selection to the closest X co-ordinate within the map grid.");

                    ImGui.SameLine();
                    ImGui.Checkbox("Y", ref CFG.Current.Toolbar_Move_to_Grid_Y);
                    ImguiUtils.ShowHelpMarker("Move the current selection to the closest Y co-ordinate within the map grid.");

                    ImGui.SameLine();
                    ImGui.Checkbox("Z", ref CFG.Current.Toolbar_Move_to_Grid_Z);
                    ImguiUtils.ShowHelpMarker("Move the current selection to the closest Z co-ordinate within the map grid.");

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
                        ImguiUtils.ShowHelpMarker("Set the current height of the map grid.");

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
                        ImguiUtils.ShowHelpMarker("Set the current height of the map grid.");
                    }
                }

                // Scramble
                if (_selectedTool == SelectedTool.Selection_Scramble)
                {
                    ImGui.Text("Scramble the current selection's position, rotation and scale by the following parameters.");
                    ImGui.Separator();
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Scramble.HintText)}");
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
                    ImguiUtils.ShowHelpMarker("Include the X co-ordinate of the selection's Position in the scramble.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinPosX", ref randomOffsetMin_Pos_X);
                    ImguiUtils.ShowHelpMarker("Minimum amount to add to the position X co-ordinate.");

                    ImGui.SameLine();

                    ImGui.InputFloat("Upper Bound##offsetMaxPosX", ref randomOffsetMax_Pos_X);
                    ImguiUtils.ShowHelpMarker("Maximum amount to add to the position X co-ordinate.");

                    ImGui.Checkbox("Y##scramblePosY", ref CFG.Current.Scrambler_RandomisePosition_Y);
                    ImguiUtils.ShowHelpMarker("Include the Y co-ordinate of the selection's Position in the scramble.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinPosY", ref randomOffsetMin_Pos_Y);
                    ImguiUtils.ShowHelpMarker("Minimum amount to add to the position Y co-ordinate.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxPosY", ref randomOffsetMax_Pos_Y);
                    ImguiUtils.ShowHelpMarker("Maximum amount to add to the position Y co-ordinate.");

                    ImGui.Checkbox("Z##scramblePosZ", ref CFG.Current.Scrambler_RandomisePosition_Z);
                    ImguiUtils.ShowHelpMarker("Include the Z co-ordinate of the selection's Position in the scramble.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinPosZ", ref randomOffsetMin_Pos_Z);
                    ImguiUtils.ShowHelpMarker("Minimum amount to add to the position Z co-ordinate.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxPosZ", ref randomOffsetMax_Pos_Z);
                    ImguiUtils.ShowHelpMarker("Maximum amount to add to the position Z co-ordinate.");

                    // Rotation
                    ImGui.Text("Rotation");
                    ImGui.Checkbox("X##scrambleRotX", ref CFG.Current.Scrambler_RandomiseRotation_X);
                    ImguiUtils.ShowHelpMarker("Include the X co-ordinate of the selection's Rotation in the scramble.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinRotX", ref randomOffsetMin_Rot_X);
                    ImguiUtils.ShowHelpMarker("Minimum amount to add to the rotation X co-ordinate.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxRotX", ref randomOffsetMax_Rot_X);
                    ImguiUtils.ShowHelpMarker("Maximum amount to add to the rotation X co-ordinate.");

                    ImGui.Checkbox("Y##scrambleRotY", ref CFG.Current.Scrambler_RandomiseRotation_Y);
                    ImguiUtils.ShowHelpMarker("Include the Y co-ordinate of the selection's Rotation in the scramble.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinRotY", ref randomOffsetMin_Rot_Y);
                    ImguiUtils.ShowHelpMarker("Minimum amount to add to the rotation Y co-ordinate.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxRotY", ref randomOffsetMax_Rot_Y);
                    ImguiUtils.ShowHelpMarker("Maximum amount to add to the rotation Y co-ordinate.");

                    ImGui.Checkbox("Z##scrambleRotZ", ref CFG.Current.Scrambler_RandomiseRotation_Z);
                    ImguiUtils.ShowHelpMarker("Include the Z co-ordinate of the selection's Rotation in the scramble.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinRotZ", ref randomOffsetMin_Rot_Z);
                    ImguiUtils.ShowHelpMarker("Minimum amount to add to the rotation Z co-ordinate.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxRotZ", ref randomOffsetMax_Rot_Z);
                    ImguiUtils.ShowHelpMarker("Maximum amount to add to the rotation Z co-ordinate.");

                    // Scale
                    ImGui.Text("Scale");
                    ImGui.Checkbox("X##scrambleScaleX", ref CFG.Current.Scrambler_RandomiseScale_X);
                    ImguiUtils.ShowHelpMarker("Include the X co-ordinate of the selection's Scale in the scramble.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinScaleX", ref randomOffsetMin_Scale_X);
                    ImguiUtils.ShowHelpMarker("Minimum amount to add to the scale X co-ordinate.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxScaleX", ref randomOffsetMax_Scale_X);
                    ImguiUtils.ShowHelpMarker("Maximum amount to add to the scale X co-ordinate.");

                    ImGui.Checkbox("Y##scrambleScaleY", ref CFG.Current.Scrambler_RandomiseScale_Y);
                    ImguiUtils.ShowHelpMarker("Include the Y co-ordinate of the selection's Scale in the scramble.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinScaleY", ref randomOffsetMin_Scale_Y);
                    ImguiUtils.ShowHelpMarker("Minimum amount to add to the scale Y co-ordinate.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxScaleY", ref randomOffsetMax_Scale_Y);
                    ImguiUtils.ShowHelpMarker("Maximum amount to add to the scale Y co-ordinate.");

                    ImGui.Checkbox("Z##scrambleScaleZ", ref CFG.Current.Scrambler_RandomiseScale_Z);
                    ImguiUtils.ShowHelpMarker("Include the Z co-ordinate of the selection's Scale in the scramble.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Lower Bound##offsetMinScaleZ", ref randomOffsetMin_Scale_Z);
                    ImguiUtils.ShowHelpMarker("Minimum amount to add to the scale Z co-ordinate.");

                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Upper Bound##offsetMaxScaleZ", ref randomOffsetMax_Scale_Y);
                    ImguiUtils.ShowHelpMarker("Maximum amount to add to the scale Z co-ordinate.");

                    ImGui.Checkbox("Scale Proportionally##scrambleSharedScale", ref CFG.Current.Scrambler_RandomiseScale_SharedScale);
                    ImguiUtils.ShowHelpMarker("When scrambling the scale, the Y and Z values will follow the X value, making the scaling proportional.");

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
                    ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Replicate.HintText)}");
                    ImGui.Separator();

                    if (ImGui.Checkbox("Line", ref CFG.Current.Replicator_Mode_Line))
                    {
                        CFG.Current.Replicator_Mode_Circle = false;
                        CFG.Current.Replicator_Mode_Square = false;
                        CFG.Current.Replicator_Mode_Sphere = false;
                        CFG.Current.Replicator_Mode_Box = false;
                    }
                    ImguiUtils.ShowHelpMarker("Replicate the first selection in the Line shape.");

                    ImGui.SameLine();
                    if (ImGui.Checkbox("Circle", ref CFG.Current.Replicator_Mode_Circle))
                    {
                        CFG.Current.Replicator_Mode_Line = false;
                        CFG.Current.Replicator_Mode_Square = false;
                        CFG.Current.Replicator_Mode_Sphere = false;
                        CFG.Current.Replicator_Mode_Box = false;
                    }
                    ImguiUtils.ShowHelpMarker("Replicate the first selection in the Circle shape.");

                    ImGui.SameLine();
                    if (ImGui.Checkbox("Square", ref CFG.Current.Replicator_Mode_Square))
                    {
                        CFG.Current.Replicator_Mode_Circle = false;
                        CFG.Current.Replicator_Mode_Line = false;
                        CFG.Current.Replicator_Mode_Sphere = false;
                        CFG.Current.Replicator_Mode_Box = false;
                    }
                    ImguiUtils.ShowHelpMarker("Replicate the first selection in the Square shape.");

                    // WIP
                    /*
                    ImGui.SameLine();
                    if (ImGui.Checkbox("Sphere", ref CFG.Current.Replicator_Mode_Sphere))
                    {
                        CFG.Current.Replicator_Mode_Circle = false;
                        CFG.Current.Replicator_Mode_Line = false;
                        CFG.Current.Replicator_Mode_Square = false;
                        CFG.Current.Replicator_Mode_Box = false;
                    }
                    ImguiUtils.ShowHelpMarker("Replicate the first selection in the Sphere shape.");

                    ImGui.SameLine();
                    if (ImGui.Checkbox("Box", ref CFG.Current.Replicator_Mode_Box))
                    {
                        CFG.Current.Replicator_Mode_Circle = false;
                        CFG.Current.Replicator_Mode_Line = false;
                        CFG.Current.Replicator_Mode_Square = false;
                        CFG.Current.Replicator_Mode_Sphere = false;
                    }
                    ImguiUtils.ShowHelpMarker("Replicate the first selection in the Box shape.");

                    */

                    // Line
                    if (CFG.Current.Replicator_Mode_Line)
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.InputInt("Amount", ref CFG.Current.Replicator_Line_Clone_Amount);
                        ImguiUtils.ShowHelpMarker("The amount of new entities to create (from the first selection).");

                        ImGui.PushItemWidth(200);
                        ImGui.InputInt("Offset", ref CFG.Current.Replicator_Line_Position_Offset);
                        ImguiUtils.ShowHelpMarker("The distance between each newly created entity.");

                        if (ImGui.Checkbox("X", ref CFG.Current.Replicator_Line_Position_Offset_Axis_X))
                        {
                            CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                            CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                        }
                        ImguiUtils.ShowHelpMarker("Replicate on the X-axis.");

                        ImGui.SameLine();
                        if (ImGui.Checkbox("Y", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Y))
                        {
                            CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                            CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                        }
                        ImguiUtils.ShowHelpMarker("Replicate on the Y-axis.");

                        ImGui.SameLine();
                        if (ImGui.Checkbox("Z", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Z))
                        {
                            CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                            CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                        }
                        ImguiUtils.ShowHelpMarker("Replicate on the Z-axis.");

                        ImGui.Checkbox("Flip Offset Direction", ref CFG.Current.Replicator_Line_Offset_Direction_Flipped);
                        ImguiUtils.ShowHelpMarker("When enabled, the position offset will be applied in the opposite direction.");
                    }

                    // Circle
                    if (CFG.Current.Replicator_Mode_Circle)
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.InputInt("Size", ref CFG.Current.Replicator_Circle_Size);
                        ImguiUtils.ShowHelpMarker("The number of points within the circle on which the entities are placed.");

                        if (ImGui.Button("Switch"))
                        {
                            CFG.Current.Replicator_Circle_Radius_Specific_Input = !CFG.Current.Replicator_Circle_Radius_Specific_Input;
                        }
                        ImGui.SameLine();
                        if (CFG.Current.Replicator_Circle_Radius_Specific_Input)
                        {
                            ImGui.PushItemWidth(200);
                            ImGui.InputFloat("Radius", ref CFG.Current.Replicator_Circle_Radius);
                        }
                        else
                        {
                            ImGui.PushItemWidth(200);
                            ImGui.SliderFloat("Radius", ref CFG.Current.Replicator_Circle_Radius, 0.1f, 100);
                        }
                        ImguiUtils.ShowHelpMarker("The radius of the circle on which to place the entities.");

                        if (CFG.Current.Replicator_Circle_Size < 1)
                            CFG.Current.Replicator_Circle_Size = 1;

                    }

                    // Square
                    if (CFG.Current.Replicator_Mode_Square)
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.InputInt("Size", ref CFG.Current.Replicator_Square_Size);
                        ImguiUtils.ShowHelpMarker("The number of points on one side of the square on which the entities are placed.");

                        ImGui.PushItemWidth(200);
                        ImGui.InputFloat("Width", ref CFG.Current.Replicator_Square_Width);
                        ImguiUtils.ShowHelpMarker("The width of the square on which to place the entities.");

                        ImGui.PushItemWidth(200);
                        ImGui.InputFloat("Depth", ref CFG.Current.Replicator_Square_Depth);
                        ImguiUtils.ShowHelpMarker("The depth of the square on which to place the entities.");

                        if (CFG.Current.Replicator_Square_Width < 1)
                            CFG.Current.Replicator_Square_Width = 1;

                        if (CFG.Current.Replicator_Square_Size < 2)
                            CFG.Current.Replicator_Square_Size = 2;

                        if (CFG.Current.Replicator_Square_Depth < 1)
                            CFG.Current.Replicator_Square_Depth = 1;
                    }

                    // Sphere
                    if (CFG.Current.Replicator_Mode_Sphere)
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.InputInt("Size", ref CFG.Current.Replicator_Sphere_Size);
                        ImguiUtils.ShowHelpMarker("The number of points within the sphere on which the entities are placed.");

                        if (ImGui.Button("Switch"))
                        {
                            CFG.Current.Replicator_Sphere_Horizontal_Radius_Specific_Input = !CFG.Current.Replicator_Sphere_Horizontal_Radius_Specific_Input;
                        }

                        ImGui.SameLine();
                        if (CFG.Current.Replicator_Sphere_Horizontal_Radius_Specific_Input)
                        {
                            ImGui.PushItemWidth(200);
                            ImGui.InputFloat("Horizontal Radius", ref CFG.Current.Replicator_Sphere_Horizontal_Radius);

                        }
                        else
                        {
                            ImGui.PushItemWidth(200);
                            ImGui.SliderFloat("Horizontal Radius", ref CFG.Current.Replicator_Sphere_Horizontal_Radius, 0.1f, 100);
                        }
                        ImguiUtils.ShowHelpMarker("The radius of the sphere on which to place the entities.");

                        if (ImGui.Button("Switch"))
                        {
                            CFG.Current.Replicator_Sphere_Vertical_Radius_Specific_Input = !CFG.Current.Replicator_Sphere_Vertical_Radius_Specific_Input;
                        }
                        ImGui.SameLine();
                        if (CFG.Current.Replicator_Sphere_Vertical_Radius_Specific_Input)
                        {
                            ImGui.PushItemWidth(200);
                            ImGui.InputFloat("Vertical Radius", ref CFG.Current.Replicator_Sphere_Vertical_Radius);
                        }
                        else
                        {
                            ImGui.PushItemWidth(200);
                            ImGui.SliderFloat("Vertical Radius", ref CFG.Current.Replicator_Sphere_Vertical_Radius, 0.1f, 100);
                        }
                        ImguiUtils.ShowHelpMarker("The vertical radius of the sphere on which to place the entities.");

                        if (CFG.Current.Replicator_Sphere_Size < 1)
                            CFG.Current.Replicator_Sphere_Size = 1;
                    }

                    // Box

                    // General Settings
                    ImGui.Separator();

                    ImGui.Checkbox("Apply Scramble Configuration", ref CFG.Current.Replicator_Apply_Scramble_Configuration);
                    ImguiUtils.ShowHelpMarker("When enabled, the Scramble configuration settings will be applied to the newly duplicated entities.");

                    if (Project.Type != ProjectType.DS2S && Project.Type != ProjectType.AC6)
                    {
                        ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Replicator_Increment_Entity_ID);
                        ImguiUtils.ShowHelpMarker("When enabled, the replicated entities will be given new Entity ID. If disabled, the replicated entity ID will be set to 0.");
                    }

                    if (Project.Type == ProjectType.ER)
                    {
                        ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Replicator_Increment_InstanceID);
                        ImguiUtils.ShowHelpMarker("When enabled, the duplicated entities will be given a new valid Instance ID.");
                    }

                    if (Project.Type == ProjectType.ER)
                    {
                        ImGui.Checkbox("Increment UnkPartNames for Assets", ref CFG.Current.Replicator_Increment_UnkPartNames);
                        ImguiUtils.ShowHelpMarker("When enabled, the duplicated Asset entities UnkPartNames property will be updated.");
                    }
                }

                // Entity ID
                if (_selectedTool == SelectedTool.Selection_Duplicate_Entity_ID)
                {
                    ImGui.Text("Output:");
                    ImGui.Separator();

                    if (_loadedMaps.Any())
                    {
                        string totalText = "";

                        if (entityIdentifiers.Count == 0)
                        {
                            totalText = "None";
                        }
                        else
                        {
                            foreach (var entry in entityIdentifiers)
                            {
                                totalText = totalText + entry.ToString();
                            }
                        }

                        ImGui.InputTextMultiline("##entityTextOutput", ref totalText, uint.MaxValue, new Vector2(600, 200));
                    }
                }

                ImGui.EndChild();
            }

            ImGui.End();
        }

        /// <summary>
        /// Check for duplicate Entity IDs
        /// </summary>
        public void CheckDuplicateEntityIdentifier()
        {
            entityIdentifiers = new List<string>();

            HashSet<uint> vals = new();
            string badVals = "";
            foreach (var loadedMap in _loadedMaps)
            {
                foreach (var e in loadedMap?.Objects)
                {
                    var val = PropFinderUtil.FindPropertyValue("EntityID", e.WrappedObject);
                    if (val == null)
                        continue;

                    uint entUint;
                    if (val is int entInt)
                        entUint = (uint)entInt;
                    else
                        entUint = (uint)val;

                    if (entUint == 0 || entUint == uint.MaxValue)
                        continue;
                    if (!vals.Add(entUint))
                        entityIdentifiers.Add($"{entUint}");
                }
            }
        }

        /// <summary>
        /// Create a new map object
        /// </summary>
        public void CreateNewMapObject()
        {
            var map = (Map)_loadedMaps.ElementAt(_createEntityMapIndex);

            if (CFG.Current.Toolbar_Create_Light)
            {
                foreach (Entity btl in map.BTLParents)
                {
                    AddNewEntity(typeof(BTL.Light), MsbEntity.MsbEntityType.Light, map, btl);
                }
            }
            if (CFG.Current.Toolbar_Create_Part)
            {
                AddNewEntity(_createPartSelectedType, MsbEntity.MsbEntityType.Part, map);
            }
            if (CFG.Current.Toolbar_Create_Region)
            {
                AddNewEntity(_createRegionSelectedType, MsbEntity.MsbEntityType.Region, map);
            }
            if (CFG.Current.Toolbar_Create_Event)
            {
                AddNewEntity(_createEventSelectedType, MsbEntity.MsbEntityType.Event, map);
            }
        }

        /// <summary>
        /// Adds a new entity to the targeted map. If no parent is specified, RootObject will be used.
        /// </summary>
        private void AddNewEntity(Type typ, MsbEntity.MsbEntityType etype, Map map, Entity parent = null)
        {
            var newent = typ.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
            MsbEntity obj = new(map, newent, etype);

            parent ??= map.RootObject;

            AddMapObjectsAction act = new(_universe, map, _scene, new List<MsbEntity> { obj }, true, parent);
            _actionManager.ExecuteAction(act);
        }

        /// <summary>
        /// Update the patrol rendering state
        /// </summary>
        public void RenderPatrolRoutes()
        {
            if (Project.Type is not ProjectType.DS2S)
            {
                PatrolDrawManager.Generate(_universe);
            }
        }

        /// <summary>
        /// Duplicate the current selection
        /// </summary>
        public void DuplicateSelection()
        {
            CloneMapObjectsAction action = new(_universe, _scene,
                    _selection.GetFilteredSelection<MsbEntity>().ToList(), true);
            _actionManager.ExecuteAction(action);
        }

        /// <summary>
        /// Move current selection to the closest grid X, Y, Z
        /// </summary>
        public void MoveSelectionToGrid()
        {
            List<ViewportAction> actlist = new();
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
                    _selection.GetFilteredSelection<MsbEntity>().ToList(), _actionManager);
            _actionManager.ExecuteAction(action);
        }

        /// <summary>
        /// Scramble the position, rotation and scale of the current selection.
        /// </summary>
        public void ScambleSelection()
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>(o => o.HasTransform))
            {
                sel.ClearTemporaryTransform(false);
                actlist.Add(sel.GetUpdateTransformAction(GetScrambledTransform(sel), true));
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
            if (CFG.Current.Toolbar_Visibility_Target_All)
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
            List<ViewportAction> actlist = new();
            HashSet<Entity> sels = _selection.GetFilteredSelection<Entity>(o => o.HasTransform);

            Vector3 camDir = Vector3.Transform(Vector3.UnitZ, _viewport.WorldView.CameraTransform.RotationMatrix);
            Vector3 camPos = _viewport.WorldView.CameraTransform.Position;
            Vector3 targetCamPos = camPos + camDir * CFG.Current.Toolbar_Move_to_Camera_Offset;

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
            List<ViewportAction> actlist = new();
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
            List<ViewportAction> actlist = new();

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

        public void ER_DummySelection()
        {
            List<MsbEntity> sourceList = _selection.GetFilteredSelection<MsbEntity>().ToList();
            foreach (MsbEntity s in sourceList)
            {
                if (Project.Type == ProjectType.ER)
                {
                    s.SetPropertyValue("GameEditionDisable", 1);
                }
            }
        }

        public void ER_UnDummySelection()
        {
            List<MsbEntity> sourceList = _selection.GetFilteredSelection<MsbEntity>().ToList();
            foreach (MsbEntity s in sourceList)
            {
                if (Project.Type == ProjectType.ER)
                {
                    s.SetPropertyValue("GameEditionDisable", 0);
                }
            }
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
            switch (Project.Type)
            {
                case ProjectType.DES:
                    msbclass = typeof(MSBD);
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    msbclass = typeof(MSB1);
                    break;
                case ProjectType.DS2S:
                    msbclass = typeof(MSB2);
                    //break;
                    return; //idk how ds2 dummies should work
                case ProjectType.DS3:
                    msbclass = typeof(MSB3);
                    break;
                case ProjectType.BB:
                    msbclass = typeof(MSBB);
                    break;
                case ProjectType.SDT:
                    msbclass = typeof(MSBS);
                    break;
                case ProjectType.ER:
                    msbclass = typeof(MSBE);
                    break;
                case ProjectType.AC6:
                    msbclass = typeof(MSB_AC6);
                    break;
                default:
                    throw new ArgumentException("type must be valid");
            }

            List<MsbEntity> sourceList = _selection.GetFilteredSelection<MsbEntity>().ToList();

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

        /// <summary>
        /// Regenerate the MCP and MCG files used for navigation data
        /// </summary>
        private void GenerateNavigationData()
        {
            Dictionary<string, ObjectContainer> orderedMaps = _universe.LoadedObjectContainers;

            HashSet<string> idCache = new();
            foreach (var map in orderedMaps)
            {
                string mapid = map.Key;

                if (Project.Type is ProjectType.DES)
                {
                    if (mapid != "m03_01_00_99" && !mapid.StartsWith("m99"))
                    {
                        var areaId = mapid.Substring(0, 3);
                        if (idCache.Contains(areaId))
                            continue;
                        idCache.Add(areaId);

                        var areaDirectories = new List<string>();
                        foreach (var orderMap in orderedMaps)
                        {
                            if (orderMap.Key.StartsWith(areaId) && orderMap.Key != "m03_01_00_99")
                            {
                                areaDirectories.Add(Path.Combine(Project.GameRootDirectory, "map", orderMap.Key));
                            }
                        }
                        SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, toBigEndian: true);
                    }
                    else
                    {
                        var areaDirectories = new List<string> { Path.Combine(Project.GameRootDirectory, "map", mapid) };
                        SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, toBigEndian: true);
                    }
                }
                else if (Project.Type is ProjectType.DS1 or ProjectType.DS1R)
                {
                    var areaDirectories = new List<string> { Path.Combine(Project.GameRootDirectory, "map", mapid) };

                    SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, toBigEndian: false);
                }
            }

            NavigationDataProcessed = true;
        }
        /// <summary>
        /// Toggle the visibility of map objects with the specified tag
        /// </summary>
        private void ToggleObjectVisibilityByTag()
        {
            foreach (ObjectContainer m in _universe.LoadedObjectContainers.Values)
            {
                if (m == null)
                {
                    continue;
                }

                foreach (Entity obj in m.Objects)
                {
                    if (obj.IsPart())
                    {
                        foreach (var entry in ModelAliasBank.Bank.AliasNames.GetEntries("Objects"))
                        {
                            var modelName = obj.GetPropertyValue<string>("ModelName");

                            if (entry.id == modelName)
                            {
                                bool change = false;

                                foreach (var tag in entry.tags)
                                {
                                    if (tag == CFG.Current.Toolbar_Tag_Visibility_Target)
                                        change = true;
                                }

                                if (change)
                                {
                                    if (CFG.Current.Toolbar_Tag_Visibility_State_Enabled)
                                    {
                                        obj.EditorVisible = true;
                                    }
                                    if (CFG.Current.Toolbar_Tag_Visibility_State_Disabled)
                                    {
                                        obj.EditorVisible = false;
                                    }
                                }
                            }
                        }

                        foreach (var entry in ModelAliasBank.Bank.AliasNames.GetEntries("MapPieces"))
                        {
                            var entryName = $"m{entry.id.Split("_").Last()}";
                            var modelName = obj.GetPropertyValue<string>("ModelName");

                            if (entryName == modelName)
                            {
                                bool change = false;

                                foreach (var tag in entry.tags)
                                {
                                    if (tag == CFG.Current.Toolbar_Tag_Visibility_Target)
                                        change = true;
                                }

                                if (change)
                                {
                                    if (CFG.Current.Toolbar_Tag_Visibility_State_Enabled)
                                    {
                                        obj.EditorVisible = true;
                                    }
                                    if (CFG.Current.Toolbar_Tag_Visibility_State_Disabled)
                                    {
                                        obj.EditorVisible = false;
                                    }
                                }
                            }
                        }

                        obj.UpdateRenderModel();
                    }
                }
            }
        }

        /// <summary>
        /// Gets all the msb types using reflection to populate editor creation menus
        /// </summary>
        /// <param name="type">The game to collect msb types for</param>
        public void PopulateClassNames()
        {
            Type msbclass;
            switch (Project.Type)
            {
                case ProjectType.DES:
                    msbclass = typeof(MSBD);
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    msbclass = typeof(MSB1);
                    break;
                case ProjectType.DS2S:
                    msbclass = typeof(MSB2);
                    break;
                case ProjectType.DS3:
                    msbclass = typeof(MSB3);
                    break;
                case ProjectType.BB:
                    msbclass = typeof(MSBB);
                    break;
                case ProjectType.SDT:
                    msbclass = typeof(MSBS);
                    break;
                case ProjectType.ER:
                    msbclass = typeof(MSBE);
                    break;
                case ProjectType.AC6:
                    msbclass = typeof(MSB_AC6);
                    break;
                default:
                    throw new ArgumentException("type must be valid");
            }

            Type partType = msbclass.GetNestedType("Part");
            List<Type> partSubclasses = msbclass.Assembly.GetTypes()
                .Where(type => type.IsSubclassOf(partType) && !type.IsAbstract).ToList();
            _partsClasses = partSubclasses.Select(x => (x.Name, x)).ToList();

            Type regionType = msbclass.GetNestedType("Region");
            List<Type> regionSubclasses = msbclass.Assembly.GetTypes()
                .Where(type => type.IsSubclassOf(regionType) && !type.IsAbstract).ToList();
            _regionClasses = regionSubclasses.Select(x => (x.Name, x)).ToList();
            if (_regionClasses.Count == 0)
            {
                _regionClasses.Add(("Region", regionType));
            }

            Type eventType = msbclass.GetNestedType("Event");
            List<Type> eventSubclasses = msbclass.Assembly.GetTypes()
                .Where(type => type.IsSubclassOf(eventType) && !type.IsAbstract).ToList();
            _eventClasses = eventSubclasses.Select(x => (x.Name, x)).ToList();
        }

        /// <summary>
        /// Save selected object's position to Position clipboard
        /// </summary>
        public void CopyCurrentPosition(PropertyInfo prop, object obj)
        {
            CFG.Current.SavedPosition = (Vector3)prop.GetValue(obj, null);
        }

        /// <summary>
        /// Paste saved position to current selection Position property
        /// </summary>
        public void PasteSavedPosition()
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>())
            {
                actlist.Add(sel.ApplySavedPosition());
            }

            CompoundAction action = new(actlist);
            _actionManager.ExecuteAction(action);
        }

        /// <summary>
        /// Save selected object's position to Rotation clipboard
        /// </summary>
        public void CopyCurrentRotation(PropertyInfo prop, object obj)
        {
            CFG.Current.SavedRotation = (Vector3)prop.GetValue(obj, null);
        }

        /// <summary>
        /// Paste saved rotation to current selection Rotation property
        /// </summary>
        public void PasteSavedRotation()
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>())
            {
                actlist.Add(sel.ApplySavedRotation());
            }

            CompoundAction action = new(actlist);
            _actionManager.ExecuteAction(action);
        }

        /// <summary>
        /// Save selected object's scale to Scale clipboard
        /// </summary>
        public void CopyCurrentScale(PropertyInfo prop, object obj)
        {
            CFG.Current.SavedScale = (Vector3)prop.GetValue(obj, null);
        }

        /// <summary>
        /// Paste saved scale to current selection Scale property
        /// </summary>
        public void PasteSavedScale()
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>())
            {
                actlist.Add(sel.ApplySavedScale());
            }

            CompoundAction action = new(actlist);
            _actionManager.ExecuteAction(action);
        }
    }
}

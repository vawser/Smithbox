using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Keybinds;

public enum KeybindID
{
    // Viewport
    MoveForward,
    MoveBackward,
    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,
    Frame,
    Pull,
    Reset,
    Cycle_Gizmo_Translation_Mode,
    Cycle_Gizmo_Rotation_Mode,
    Cycle_Gizmo_Scale_Mode,
    Cycle_Gizmo_Origin_Mode,
    Cycle_Gizmo_Space_Mode,
    Cycle_Render_Outline_Mode,

    // Common Actions
    Save,
    Undo,
    Redo,
    Undo_Repeat,
    Redo_Repeat,
    Up,
    Down,
    Left,
    Right,

    // Contextual Actions
    SelectAll,
    Copy,
    Paste,
    Duplicate,
    Delete,
    Jump,
    Reorder_Up,
    Reorder_Down,
    Reorder_Top,
    Reorder_Bottom,
    Apply_to_All,
    Toggle_Tools_Menu,

    // Map Editor
    MapEditor_Toggle_World_Map_Menu,
    MapEditor_Reset_World_Map_Zoom_Level,
    MapEditor_Create_Map_Object,
    MapEditor_Duplicate_To_Map,
    MapEditor_Rotate_X_Axis,
    MapEditor_Rotate_Y_Axis,
    MapEditor_Rotate_Pivot_Y_Axis,
    MapEditor_Rotate_Minus_X_Axis,
    MapEditor_Rotate_Minus_Y_Axis,
    MapEditor_Rotate_Minus_Pivot_Y_Axis,
    MapEditor_Rotate_Fixed_Angle,
    MapEditor_Reset_Rotation,
    MapEditor_Scramble,
    MapEditor_Replicate,
    MapEditor_Cycle_Render_Type,
    MapEditor_Make_Dummy_Object,
    MapEditor_Make_Normal_Object,
    MapEditor_Enable_Game_Presence,
    MapEditor_Disable_Game_Presence,
    MapEditor_Visibility_Flip,
    MapEditor_Visibility_Enable,
    MapEditor_Visibility_Disable,
    MapEditor_Global_Visibility_Flip,
    MapEditor_Global_Visibility_Enable,
    MapEditor_Global_Visibility_Disable,
    MapEditor_Configure_Grid_Placement,
    MapEditor_Cycle_Selected_Grid_Type,
    MapEditor_Move_to_Primary_Grid,
    MapEditor_Move_to_Secondary_Grid,
    MapEditor_Move_to_Tertiary_Grid,
    MapEditor_SelectAll_Configurable,
    MapEditor_SelectAll_ObjectType,
    MapEditor_SelectAll_ModelName,
    MapEditor_SelectAll_Ceremony,
    MapEditor_Create_Selection_Group,
    MapEditor_Select_Group_0,
    MapEditor_Select_Group_1,
    MapEditor_Select_Group_2,
    MapEditor_Select_Group_3,
    MapEditor_Select_Group_4,
    MapEditor_Select_Group_5,
    MapEditor_Select_Group_6,
    MapEditor_Select_Group_7,
    MapEditor_Select_Group_8,
    MapEditor_Select_Group_9,
    MapEditor_Select_Group_10,
    MapEditor_Rotation_Increment_Cycle_Type,
    MapEditor_Rotation_Increment_Cycle_Type_Backwards,
    MapEditor_Position_Increment_Cycle_Type,
    MapEditor_Position_Increment_Cycle_Type_Backwards,
    MapEditor_Position_Increment_Toggle_Discrete_Mode,
    MapEditor_Position_Increment_Positive_X,
    MapEditor_Position_Increment_Negative_X,
    MapEditor_Position_Increment_Positive_Y,
    MapEditor_Position_Increment_Negative_Y,
    MapEditor_Position_Increment_Positive_Z,
    MapEditor_Position_Increment_Negative_Z,
    MapEditor_Toggle_Patrol_Route_Visuals,
    MapEditor_View_Display_Group,
    MapEditor_View_Draw_Group,
    MapEditor_Apply_Display_Group,
    MapEditor_Apply_Draw_Group,
    MapEditor_Hide_All_Display_Groups,
    MapEditor_Show_All_Display_Groups,
    MapEditor_Select_Display_Group_Highlights,
    MapEditor_Select_Collision_References,
    MapEditor_Select_Referenced_Collision,
    MapEditor_Deselect_All,

    // Model Editor

    // Param Editor
    ParamEditor_Focus_Searchbar,
    ParamEditor_Apply_Mass_Edit,
    ParamEditor_View_Mass_Edit,
    ParamEditor_Import_CSV,
    ParamEditor_Export_CSV,
    ParamEditor_Export_CSV_Names,
    ParamEditor_Export_CSV_Param,
    ParamEditor_Export_CSV_All_Rows,
    ParamEditor_Export_CSV_Modified_Rows,
    ParamEditor_Export_CSV_Selected_Rows,
    ParamEditor_Reload_All_Params,
    ParamEditor_Reload_Selected_Param,
    ParamEditor_RowList_Sort_Rows,
    ParamEditor_RowList_Jump_to_Row_ID,
    ParamEditor_RowList_Inherit_Referenced_Row_Name,

    // Text Editor
    TextEditor_Focus_Searchbar,
    TextEditor_Create_New_Entry,
    TextEditor_Configurable_Duplicate,

    // Gparam Editor
    GparamEditor_Execute_Quick_Edit,
    GparamEditor_Generate_Quick_Edit,
    GparamEditor_Clear_Quick_Edit,

    // Material Editor

    // Texture Viewer
    TextureViewer_Export_Texture,
    TextureViewer_Reset_Zoom_Level
}

public static class DefaultKeyBindings
{
    public static void CreateDefaultBindings()
    {
        // Viewport
        InputManager.Bind(KeybindID.MoveForward, new() { Key = Key.W });
        InputManager.Bind(KeybindID.MoveBackward, new() { Key = Key.S });
        InputManager.Bind(KeybindID.MoveLeft, new() { Key = Key.A });
        InputManager.Bind(KeybindID.MoveRight, new() { Key = Key.D });
        InputManager.Bind(KeybindID.MoveUp, new() { Key = Key.Q });
        InputManager.Bind(KeybindID.MoveDown, new() { Key = Key.E });

        InputManager.Bind(KeybindID.Frame, new() { Key = Key.F });
        InputManager.Bind(KeybindID.Pull, new() { Key = Key.X });
        InputManager.Bind(KeybindID.Reset, new() { Key = Key.R });

        InputManager.Bind(KeybindID.Cycle_Gizmo_Translation_Mode, new() { Key = Key.W, Shift = true });
        InputManager.Bind(KeybindID.Cycle_Gizmo_Rotation_Mode, new() { Key = Key.E, Shift = true });
        InputManager.Bind(KeybindID.Cycle_Gizmo_Scale_Mode, new() { Key = Key.G, Shift = true });

        InputManager.Bind(KeybindID.Cycle_Gizmo_Origin_Mode, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.Cycle_Gizmo_Space_Mode, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.Cycle_Render_Outline_Mode, new() { Key = Key.Number1, Alt = true });

        // Common
        InputManager.Bind(KeybindID.Save, new() { Key = Key.S, Ctrl = true });
        InputManager.Bind(KeybindID.Undo, new() { Key = Key.Z, Ctrl = true });
        InputManager.Bind(KeybindID.Redo, new() { Key = Key.Y, Ctrl = true });
        InputManager.Bind(KeybindID.Undo_Repeat, new() { Key = Key.Z, Ctrl = true, Alt = true });
        InputManager.Bind(KeybindID.Redo_Repeat, new() { Key = Key.Y, Ctrl = true, Alt = true });
        InputManager.Bind(KeybindID.Up, new() { Key = Key.Up });
        InputManager.Bind(KeybindID.Down, new() { Key = Key.Down });
        InputManager.Bind(KeybindID.Left, new() { Key = Key.Left });
        InputManager.Bind(KeybindID.Right, new() { Key = Key.Right });

        // Contextual
        InputManager.Bind(KeybindID.SelectAll, new() { Key = Key.A, Ctrl = true });
        InputManager.Bind(KeybindID.Copy, new() { Key = Key.C, Ctrl = true });
        InputManager.Bind(KeybindID.Paste, new() { Key = Key.V, Ctrl = true });
        InputManager.Bind(KeybindID.Duplicate, new() { Key = Key.D, Ctrl = true });
        InputManager.Bind(KeybindID.Delete, new() { Key = Key.Delete });
        InputManager.Bind(KeybindID.Jump, new() { Key = Key.Q, Ctrl = true });

        InputManager.Bind(KeybindID.Reorder_Up, new() { Key = Key.Up, Alt = true });
        InputManager.Bind(KeybindID.Reorder_Down, new() { Key = Key.Down, Alt = true });
        InputManager.Bind(KeybindID.Reorder_Top, new() { Key = Key.Up, Ctrl = true, Alt = true });
        InputManager.Bind(KeybindID.Reorder_Bottom, new() { Key = Key.Down, Ctrl = true, Alt = true });

        InputManager.Bind(KeybindID.Apply_to_All, new() { Key = Key.A, Shift = true });
        InputManager.Bind(KeybindID.Toggle_Tools_Menu, new() { Key = Key.Keypad1, Ctrl = true });

        // Map Editor
        InputManager.Bind(KeybindID.MapEditor_Toggle_World_Map_Menu, new() { Key = Key.M });
        InputManager.Bind(KeybindID.MapEditor_Reset_World_Map_Zoom_Level, new() { Key = Key.R });

        InputManager.Bind(KeybindID.MapEditor_Create_Map_Object, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Duplicate_To_Map, new() { Key = Key.D, Alt = true });

        InputManager.Bind(KeybindID.MapEditor_Rotate_X_Axis, new() { Key = Key.R, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Rotate_Y_Axis, new() { Key = Key.R, Shift = true });
        InputManager.Bind(KeybindID.MapEditor_Rotate_Pivot_Y_Axis, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Rotate_Minus_X_Axis, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Rotate_Minus_Y_Axis, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Rotate_Minus_Pivot_Y_Axis, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Rotate_Fixed_Angle, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Reset_Rotation, new() { Key = Key.R, Ctrl = true });

        InputManager.Bind(KeybindID.MapEditor_Scramble, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Replicate, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Cycle_Render_Type, new() { Key = Key.O });

        InputManager.Bind(KeybindID.MapEditor_Make_Dummy_Object, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Make_Normal_Object, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Enable_Game_Presence, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Disable_Game_Presence, new() { Key = Key.Unknown });

        InputManager.Bind(KeybindID.MapEditor_Visibility_Flip, new() { Key = Key.Z, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Visibility_Enable, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Visibility_Disable, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Global_Visibility_Flip, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Global_Visibility_Enable, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Global_Visibility_Disable, new() { Key = Key.Unknown });

        InputManager.Bind(KeybindID.MapEditor_Configure_Grid_Placement, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Cycle_Selected_Grid_Type, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Move_to_Primary_Grid, new() { Key = Key.X, Shift = true });
        InputManager.Bind(KeybindID.MapEditor_Move_to_Secondary_Grid, new() { Key = Key.X, Ctrl = true });
        InputManager.Bind(KeybindID.MapEditor_Move_to_Tertiary_Grid, new() { Key = Key.X, Alt = true });

        InputManager.Bind(KeybindID.MapEditor_SelectAll_Configurable, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_SelectAll_ObjectType, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_SelectAll_ModelName, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_SelectAll_Ceremony, new() { Key = Key.Unknown });

        InputManager.Bind(KeybindID.MapEditor_Create_Selection_Group, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Select_Group_0, new() { Key = Key.Number0, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Select_Group_1, new() { Key = Key.Number1, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Select_Group_2, new() { Key = Key.Number2, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Select_Group_3, new() { Key = Key.Number3, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Select_Group_4, new() { Key = Key.Number4, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Select_Group_5, new() { Key = Key.Number5, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Select_Group_6, new() { Key = Key.Unknown, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Select_Group_7, new() { Key = Key.Unknown, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Select_Group_8, new() { Key = Key.Unknown, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Select_Group_9, new() { Key = Key.Unknown, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Select_Group_10, new() { Key = Key.Unknown, Alt = true });

        InputManager.Bind(KeybindID.MapEditor_Rotation_Increment_Cycle_Type, new() { Key = Key.F2 });
        InputManager.Bind(KeybindID.MapEditor_Rotation_Increment_Cycle_Type_Backwards, new() { Key = Key.Unknown });

        InputManager.Bind(KeybindID.MapEditor_Position_Increment_Cycle_Type, new() { Key = Key.F1 });
        InputManager.Bind(KeybindID.MapEditor_Position_Increment_Toggle_Discrete_Mode, new() { Key = Key.F3 });
        InputManager.Bind(KeybindID.MapEditor_Position_Increment_Positive_X, new() { Key = Key.Right, Ctrl = true });
        InputManager.Bind(KeybindID.MapEditor_Position_Increment_Negative_X, new() { Key = Key.Left, Ctrl = true });
        InputManager.Bind(KeybindID.MapEditor_Position_Increment_Positive_Y, new() { Key = Key.Up, Ctrl = true });
        InputManager.Bind(KeybindID.MapEditor_Position_Increment_Negative_Y, new() { Key = Key.Down, Ctrl = true });
        InputManager.Bind(KeybindID.MapEditor_Position_Increment_Positive_Z, new() { Key = Key.Up, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Position_Increment_Negative_Z, new() { Key = Key.Down, Alt = true });

        InputManager.Bind(KeybindID.MapEditor_Toggle_Patrol_Route_Visuals, new() { Key = Key.Unknown });

        InputManager.Bind(KeybindID.MapEditor_View_Display_Group, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_View_Draw_Group, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Apply_Display_Group, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Apply_Draw_Group, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Hide_All_Display_Groups, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Show_All_Display_Groups, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Select_Display_Group_Highlights, new() { Key = Key.Unknown });

        InputManager.Bind(KeybindID.MapEditor_Select_Collision_References, new() { Key = Key.U, Ctrl = true });
        InputManager.Bind(KeybindID.MapEditor_Select_Referenced_Collision, new() { Key = Key.U, Alt = true });

        InputManager.Bind(KeybindID.MapEditor_Deselect_All, new() { Key = Key.Escape });

        // Model Editor

        // Param Editor
        InputManager.Bind(KeybindID.ParamEditor_Focus_Searchbar, new() { Key = Key.F, Ctrl = true });

        InputManager.Bind(KeybindID.ParamEditor_Apply_Mass_Edit, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.ParamEditor_View_Mass_Edit, new() { Key = Key.Unknown });

        InputManager.Bind(KeybindID.ParamEditor_Import_CSV, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.ParamEditor_Export_CSV, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.ParamEditor_Export_CSV_Names, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.ParamEditor_Export_CSV_Param, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.ParamEditor_Export_CSV_All_Rows, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.ParamEditor_Export_CSV_Modified_Rows, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.ParamEditor_Export_CSV_Selected_Rows, new() { Key = Key.Unknown });

        InputManager.Bind(KeybindID.ParamEditor_Reload_All_Params, new() { Key = Key.F6 });
        InputManager.Bind(KeybindID.ParamEditor_Reload_Selected_Param, new() { Key = Key.F5 });

        InputManager.Bind(KeybindID.ParamEditor_RowList_Sort_Rows, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.ParamEditor_RowList_Jump_to_Row_ID, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.ParamEditor_RowList_Inherit_Referenced_Row_Name, new() { Key = Key.Unknown });

        // Text Editor
        InputManager.Bind(KeybindID.TextEditor_Focus_Searchbar, new() { Key = Key.F, Ctrl = true });
        InputManager.Bind(KeybindID.TextEditor_Configurable_Duplicate, new() { Key = Key.D, Alt = true });
        InputManager.Bind(KeybindID.TextEditor_Create_New_Entry, new() { Key = Key.Insert });

        // Gparam Editor
        InputManager.Bind(KeybindID.GparamEditor_Execute_Quick_Edit, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.GparamEditor_Generate_Quick_Edit, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.GparamEditor_Clear_Quick_Edit, new() { Key = Key.Unknown });

        // Material Editor

        // Texture Viewer
        InputManager.Bind(KeybindID.TextureViewer_Export_Texture, new() { Key = Key.X, Ctrl = true });
        InputManager.Bind(KeybindID.TextureViewer_Reset_Zoom_Level, new() { Key = Key.R, Ctrl = true });
    }
}

public static class KeybindMetadata
{
    public static readonly Dictionary<KeybindID, (string, string)> Presentation
        = new()
    {
        // Viewport
        { KeybindID.MoveForward, (
            "Move Forward",
            "Within the viewport, this will move the camera forward."
        ) },
        { KeybindID.MoveBackward, (
            "Move Backward",
            "Within the viewport, this will move the camera backward."
        ) },
        { KeybindID.MoveLeft, (
            "Move Left",
            "Within the viewport, this will move the camera to the left."
        ) },
        { KeybindID.MoveRight, (
            "Move Right",
            "Within the viewport, this will move the camera to the right."
        ) },
        { KeybindID.MoveUp, (
            "Move Up",
            "Within the viewport, this will move the camera upwards."
        ) },
        { KeybindID.MoveDown, (
            "Move Down",
            "Within the viewport, this will move the camera downwards."
        ) },
        { KeybindID.Frame, (
            "Frame",
            "Within the viewport, this will frame the currently selected viewport object in the centre of the screen."
        ) },
        { KeybindID.Pull, (
            "Pull",
            "Within the viewport, this will move the currently selected viewport object to the camera's location."
        ) },
        { KeybindID.Reset, (
            "Reset",
            "Within the viewport, this reset the camera's location."
        ) },
        { KeybindID.Cycle_Gizmo_Translation_Mode, (
            "Set to Gizmo Translation Mode",
            "Within the viewport, this will switch the viewport object gizmo to 'Translate'."
        ) },
        { KeybindID.Cycle_Gizmo_Rotation_Mode, (
            "Set to Gizmo Rotation Mode",
            "Within the viewport, this will switch the viewport object gizmo to 'Rotate'."
        ) },
        { KeybindID.Cycle_Gizmo_Scale_Mode, (
            "Set to Gizmo Scale Mode",
            "Within the viewport, this will switch the viewport object gizmo to 'Scale'."
        ) },
        { KeybindID.Cycle_Gizmo_Origin_Mode, (
            "Cycle Gizmo Origin Mode",
            "Within the viewport, this will switch the viewport object gizmo origin type."
        ) },
        { KeybindID.Cycle_Gizmo_Space_Mode, (
            "Cycle Gizmo Worldspace Mode",
            "Within the viewport, this will switch the viewport object gizmo worldspace type."
        ) },
        { KeybindID.Cycle_Render_Outline_Mode, (
            "Cycle Render Outline Mode",
            "Within the viewport, this will switch the currently selected viewport object's rendering type."
        ) },

        // Common
        { KeybindID.Save, (
            "Save",
            "This will save within the currently focused editor."
        ) },
        { KeybindID.Undo, (
            "Undo",
            "This will undo the most recent action."
        ) },
        { KeybindID.Redo, (
            "Redo",
            "This will redo the most recently undone action."
        ) },
        { KeybindID.Undo_Repeat, (
            "Undo (continuous)",
            "This will undo the most recent action repeatly whilst held."
        ) },
        { KeybindID.Redo_Repeat, (
            "Redo (continuous)",
            "This will redo the most recently undone action repeatly whilst held."
        ) },
        { KeybindID.Up, (
            "Up",
            "Within a list, this will move the selection one space up."
        ) },
        { KeybindID.Down, (
            "Down",
            "Within a list, this will move the selection one space down."
        ) },
        { KeybindID.Left, (
            "Left",
            "Within a list, this will move the selection one space to the left."
        ) },
        { KeybindID.Right, (
            "Right",
            "Within a list, this will move the selection one space to the right."
        ) },

        // Contextual
        { KeybindID.SelectAll, (
            "Select All",
            "This will select all entries within a list."
        ) },
        { KeybindID.Copy, (
            "Copy",
            "Copy the currently selected entries into a clipboard."
        ) },
        { KeybindID.Paste, (
            "Paste",
            "Paste the entries that reside within the clipboard into the list."
        ) },
        { KeybindID.Duplicate, (
            "Duplicate",
            "When an entry is selected, this will duplicate the entry within the list."
        ) },
        { KeybindID.Delete, (
            "Delete",
            "When an entry is selected, this will delete the entry from the list."
        ) },
        { KeybindID.Jump, (
            "Jump",
            "When an entry is selected, this will focus the entry to the centre of the user's screen."
        ) },
        { KeybindID.Reorder_Up, (
            "Move Up",
            "When in a list, the selected entry is moved up one space."
        ) },
        { KeybindID.Reorder_Down, (
            "Move Down",
            "When in a list, the selected entry is moved down one space."
        ) },
        { KeybindID.Reorder_Top, (
            "Move to Top",
            "When in a list, the selected entry is moved to the top of the list."
        ) },
        { KeybindID.Reorder_Bottom, (
            "Move to Bottom",
            "When in a list, the selected entry is moved to the bottom of the list."
        ) },
        { KeybindID.Apply_to_All, (
            "Apply to All",
            "When in a list, holding this button will cause the action to apply to all entries."
        ) },
        { KeybindID.Toggle_Tools_Menu, (
            "Toggle Tools Menu",
            "Toggle the visibility of the Tools menu within the current editor."
        ) },

        // Map 
        { KeybindID.MapEditor_Toggle_World_Map_Menu, (
            "Toggle World Map Menu",
            ""
        ) },
        { KeybindID.MapEditor_Reset_World_Map_Zoom_Level, (
            "Reset World Map Zoom Level",
            ""
        ) },
        { KeybindID.MapEditor_Create_Map_Object, (
            "Create Map Object",
            ""
        ) },
        { KeybindID.MapEditor_Duplicate_To_Map, (
            "Duplicate to Map",
            ""
        ) },
        { KeybindID.MapEditor_Rotate_X_Axis, (
            "Rotate Selection (+ x-axis)",
            ""
        ) },
        { KeybindID.MapEditor_Rotate_Y_Axis, (
            "Rotate Selection (+ y-axis)",
            ""
        ) },
        { KeybindID.MapEditor_Rotate_Pivot_Y_Axis, (
            "Pivot Selection (+ y-axis)",
            ""
        ) },
        { KeybindID.MapEditor_Rotate_Minus_X_Axis, (
            "Rotate Selection (- x-axis)",
            ""
        ) },
        { KeybindID.MapEditor_Rotate_Minus_Y_Axis, (
            "Rotate Selection (- y-axis)",
            ""
        ) },
        { KeybindID.MapEditor_Rotate_Minus_Pivot_Y_Axis, (
            "Pivot Selection (- y-axis)",
            ""
        ) },
        { KeybindID.MapEditor_Rotate_Fixed_Angle, (
            "Rotate Selection (fixed angle)",
            ""
        ) },
        { KeybindID.MapEditor_Reset_Rotation, (
            "Reset Selection Rotation",
            ""
        ) },
        { KeybindID.MapEditor_Scramble, (
            "Scramble Selection",
            ""
        ) },
        { KeybindID.MapEditor_Replicate, (
            "Replicate Selection",
            ""
        ) },
        { KeybindID.MapEditor_Cycle_Render_Type, (
            "Cycle Render Type",
            ""
        ) },
        { KeybindID.MapEditor_Make_Dummy_Object, (
            "Set Map Object to Dummy",
            ""
        ) },
        { KeybindID.MapEditor_Make_Normal_Object, (
            "Set Map Object to Normal",
            ""
        ) },
        { KeybindID.MapEditor_Enable_Game_Presence, (
            "Enable Map Object Game Presence",
            ""
        ) },
        { KeybindID.MapEditor_Disable_Game_Presence, (
            "Disable Map Object Game Presence",
            ""
        ) },
        { KeybindID.MapEditor_Visibility_Flip, (
            "Flip Selection Visibility",
            ""
        ) },
        { KeybindID.MapEditor_Visibility_Enable, (
            "Enable Selection Visibility",
            ""
        ) },
        { KeybindID.MapEditor_Visibility_Disable, (
            "Disable Selection Visibility",
            ""
        ) },
        { KeybindID.MapEditor_Global_Visibility_Flip, (
            "Flip Visibility for All",
            ""
        ) },
        { KeybindID.MapEditor_Global_Visibility_Enable, (
            "Enable Visibility for All",
            ""
        ) },
        { KeybindID.MapEditor_Global_Visibility_Disable, (
            "Disable Visibility for All",
            ""
        ) },
        { KeybindID.MapEditor_Configure_Grid_Placement, (
            "Configure Grid Placement",
            ""
        ) },
        { KeybindID.MapEditor_Cycle_Selected_Grid_Type, (
            "Cycle Selected Grid",
            ""
        ) },
        { KeybindID.MapEditor_Move_to_Primary_Grid, (
            "Move Selection to Primary Grid",
            ""
        ) },
        { KeybindID.MapEditor_Move_to_Secondary_Grid, (
            "Move Selection to Secondary Grid",
            ""
        ) },
        { KeybindID.MapEditor_Move_to_Tertiary_Grid, (
            "Move Selection to Tertiary Grid",
            ""
        ) },
        { KeybindID.MapEditor_SelectAll_Configurable, (
            "Select All (Configurable)",
            ""
        ) },
        { KeybindID.MapEditor_SelectAll_ObjectType, (
            "Select All (Object Type)",
            ""
        ) },
        { KeybindID.MapEditor_SelectAll_ModelName, (
            "Select All (Model Name)",
            ""
        ) },
        { KeybindID.MapEditor_SelectAll_Ceremony, (
            "Select All (Ceremony)",
            ""
        ) },
        { KeybindID.MapEditor_Create_Selection_Group, (
            "Create Selection Group",
            ""
        ) },
        { KeybindID.MapEditor_Select_Group_0, (
            "Select Selection Group 0",
            ""
        ) },
        { KeybindID.MapEditor_Select_Group_1, (
            "Select Selection Group 1",
            ""
        ) },
        { KeybindID.MapEditor_Select_Group_2, (
            "Select Selection Group 2",
            ""
        ) },
        { KeybindID.MapEditor_Select_Group_3, (
            "Select Selection Group 3",
            ""
        ) },
        { KeybindID.MapEditor_Select_Group_4, (
            "Select Selection Group 4",
            ""
        ) },
        { KeybindID.MapEditor_Select_Group_5, (
            "Select Selection Group 5",
            ""
        ) },
        { KeybindID.MapEditor_Select_Group_6, (
            "Select Selection Group 6",
            ""
        ) },
        { KeybindID.MapEditor_Select_Group_7, (
            "Select Selection Group 7",
            ""
        ) },
        { KeybindID.MapEditor_Select_Group_8, (
            "Select Selection Group 8",
            ""
        ) },
        { KeybindID.MapEditor_Select_Group_9, (
            "Select Selection Group 9",
            ""
        ) },
        { KeybindID.MapEditor_Select_Group_10, (
            "Select Selection Group 10",
            ""
        ) },
        { KeybindID.MapEditor_Rotation_Increment_Cycle_Type, (
            "Cycle Rotation Increment Type",
            ""
        ) },
        { KeybindID.MapEditor_Rotation_Increment_Cycle_Type_Backwards, (
            "Cycle Rotation Increment Type Backwards",
            ""
        ) },
        { KeybindID.MapEditor_Position_Increment_Cycle_Type, (
            "Cycle Position Increment Type",
            ""
        ) },
        { KeybindID.MapEditor_Position_Increment_Cycle_Type_Backwards, (
            "Cycle Position Increment Type Backwards",
            ""
        ) },
        { KeybindID.MapEditor_Position_Increment_Toggle_Discrete_Mode, (
            "Toggle Position Increment Discrete Mode",
            ""
        ) },
        { KeybindID.MapEditor_Position_Increment_Positive_X, (
            "Position Increment Move (+x)",
            ""
        ) },
        { KeybindID.MapEditor_Position_Increment_Negative_X, (
            "Position Increment Move (-x)",
            ""
        ) },
        { KeybindID.MapEditor_Position_Increment_Positive_Y, (
            "Position Increment Move (+y)",
            ""
        ) },
        { KeybindID.MapEditor_Position_Increment_Negative_Y, (
            "Position Increment Move (-y)",
            ""
        ) },
        { KeybindID.MapEditor_Position_Increment_Positive_Z, (
            "Position Increment Move (+z)",
            ""
        ) },
        { KeybindID.MapEditor_Position_Increment_Negative_Z, (
            "Position Increment Move (-z)",
            ""
        ) },
        { KeybindID.MapEditor_Toggle_Patrol_Route_Visuals, (
            "Toggle Patrol Route Visuals",
            ""
        ) },
        { KeybindID.MapEditor_View_Display_Group, (
            "View Display Group for Selection",
            ""
        ) },
        { KeybindID.MapEditor_View_Draw_Group, (
            "View Draw Group for Selection",
            ""
        ) },
        { KeybindID.MapEditor_Apply_Display_Group, (
            "Apply Display Group to Selection",
            ""
        ) },
        { KeybindID.MapEditor_Apply_Draw_Group, (
            "Apply Draw Group to Selection",
            ""
        ) },
        { KeybindID.MapEditor_Hide_All_Display_Groups, (
            "Hide All Display Groups",
            ""
        ) },
        { KeybindID.MapEditor_Show_All_Display_Groups, (
            "Show All Display Groups",
            ""
        ) },
        { KeybindID.MapEditor_Select_Display_Group_Highlights, (
            "Select Display Group Highlights",
            ""
        ) },
        { KeybindID.MapEditor_Select_Collision_References, (
            "Select Collision References",
            "Selects all map objects that reference the currently selected collision(s)."
        ) },
        { KeybindID.MapEditor_Select_Referenced_Collision, (
            "Select Referenced Collision",
            "Selects the collisions referenced by the currently selected part(s)."
        ) },
        { KeybindID.MapEditor_Deselect_All, (
            "Deselect All",
            ""
        ) },

        // Model Editor

        // Param Editor
        { KeybindID.ParamEditor_Focus_Searchbar, (
            "Focus Searchbar",
            ""
        ) },
        { KeybindID.ParamEditor_Apply_Mass_Edit, (
            "Apply Mass Edit",
            ""
        ) },
        { KeybindID.ParamEditor_View_Mass_Edit, (
            "View Mass Edit",
            ""
        ) },
        { KeybindID.ParamEditor_Import_CSV, (
            "Import CSV",
            ""
        ) },
        { KeybindID.ParamEditor_Export_CSV, (
            "Export CSV",
            ""
        ) },
        { KeybindID.ParamEditor_Export_CSV_Names, (
            "Export CSV (names)",
            ""
        ) },
        { KeybindID.ParamEditor_Export_CSV_Param, (
            "Export CSV (param)",
            ""
        ) },
        { KeybindID.ParamEditor_Export_CSV_All_Rows, (
            "Export CSV (all rows)",
            ""
        ) },
        { KeybindID.ParamEditor_Export_CSV_Modified_Rows, (
            "Export CSV (modified rows)",
            ""
        ) },
        { KeybindID.ParamEditor_Export_CSV_Selected_Rows, (
            "Export CSV (selected rows)",
            ""
        ) },
        { KeybindID.ParamEditor_Reload_All_Params, (
            "Reload All Params",
            ""
        ) },
        { KeybindID.ParamEditor_Reload_Selected_Param, (
            "Reload Selected Param",
            ""
        ) },
        { KeybindID.ParamEditor_RowList_Sort_Rows, (
            "Sort Rows",
            ""
        ) },
        { KeybindID.ParamEditor_RowList_Jump_to_Row_ID, (
            "Jump to Row ID",
            ""
        ) },
        { KeybindID.ParamEditor_RowList_Inherit_Referenced_Row_Name, (
            "Inherit Referenced Row Name",
            ""
        ) },

        // Text Editor
        { KeybindID.TextEditor_Focus_Searchbar, (
            "Focus Search Bar",
            ""
        ) },
        { KeybindID.TextEditor_Configurable_Duplicate, (
            "Configurable Duplicate",
            ""
        ) },
        { KeybindID.TextEditor_Create_New_Entry, (
            "Create New Entry",
            ""
        ) },

        // Gparam Editor
        { KeybindID.GparamEditor_Execute_Quick_Edit, (
            "Execute Quick Edit",
            ""
        ) },
        { KeybindID.GparamEditor_Generate_Quick_Edit, (
            "Generate Quick Edit",
            ""
        ) },
        { KeybindID.GparamEditor_Clear_Quick_Edit, (
            "Clear Quick Edit",
            ""
        ) },

        // Material Editor

        // Texture Viewer
        { KeybindID.TextureViewer_Export_Texture, (
            "Export Texture",
            ""
        ) },
        { KeybindID.TextureViewer_Reset_Zoom_Level, (
            "Reset Zoom Level",
            ""
        ) }
    };

    public static readonly Dictionary<KeybindID, InputCategory> Category
        = new()
    {
        // Viewport
        { KeybindID.MoveForward, InputCategory.Viewport },
        { KeybindID.MoveBackward, InputCategory.Viewport },
        { KeybindID.MoveLeft, InputCategory.Viewport },
        { KeybindID.MoveRight, InputCategory.Viewport },
        { KeybindID.MoveUp, InputCategory.Viewport },
        { KeybindID.MoveDown, InputCategory.Viewport },
        { KeybindID.Frame, InputCategory.Viewport },
        { KeybindID.Pull, InputCategory.Viewport },
        { KeybindID.Reset, InputCategory.Viewport },
        { KeybindID.Cycle_Gizmo_Translation_Mode, InputCategory.Viewport },
        { KeybindID.Cycle_Gizmo_Rotation_Mode, InputCategory.Viewport },
        { KeybindID.Cycle_Gizmo_Scale_Mode, InputCategory.Viewport },
        { KeybindID.Cycle_Gizmo_Origin_Mode, InputCategory.Viewport },
        { KeybindID.Cycle_Gizmo_Space_Mode, InputCategory.Viewport },
        { KeybindID.Cycle_Render_Outline_Mode, InputCategory.Viewport },

        // Common
        { KeybindID.Save, InputCategory.Common },
        { KeybindID.Undo, InputCategory.Common },
        { KeybindID.Redo, InputCategory.Common },
        { KeybindID.Undo_Repeat, InputCategory.Common },
        { KeybindID.Redo_Repeat, InputCategory.Common },
        { KeybindID.Up, InputCategory.Common },
        { KeybindID.Down, InputCategory.Common },
        { KeybindID.Left, InputCategory.Common },
        { KeybindID.Right, InputCategory.Common },

        // Contextual
        { KeybindID.SelectAll, InputCategory.Contextual },
        { KeybindID.Copy, InputCategory.Contextual },
        { KeybindID.Paste, InputCategory.Contextual },
        { KeybindID.Duplicate, InputCategory.Contextual },
        { KeybindID.Delete, InputCategory.Contextual },
        { KeybindID.Jump, InputCategory.Contextual },
        { KeybindID.Reorder_Up, InputCategory.Contextual },
        { KeybindID.Reorder_Down, InputCategory.Contextual },
        { KeybindID.Reorder_Top, InputCategory.Contextual },
        { KeybindID.Reorder_Bottom, InputCategory.Contextual },
        { KeybindID.Apply_to_All, InputCategory.Contextual },
        { KeybindID.Toggle_Tools_Menu, InputCategory.Contextual },

        // Map 
        { KeybindID.MapEditor_Toggle_World_Map_Menu, InputCategory.MapEditor },
        { KeybindID.MapEditor_Reset_World_Map_Zoom_Level, InputCategory.MapEditor },
        { KeybindID.MapEditor_Create_Map_Object, InputCategory.MapEditor },
        { KeybindID.MapEditor_Duplicate_To_Map, InputCategory.MapEditor },
        { KeybindID.MapEditor_Rotate_X_Axis, InputCategory.MapEditor },
        { KeybindID.MapEditor_Rotate_Y_Axis, InputCategory.MapEditor },
        { KeybindID.MapEditor_Rotate_Pivot_Y_Axis, InputCategory.MapEditor },
        { KeybindID.MapEditor_Rotate_Minus_X_Axis, InputCategory.MapEditor },
        { KeybindID.MapEditor_Rotate_Minus_Y_Axis, InputCategory.MapEditor },
        { KeybindID.MapEditor_Rotate_Minus_Pivot_Y_Axis, InputCategory.MapEditor },
        { KeybindID.MapEditor_Rotate_Fixed_Angle, InputCategory.MapEditor },
        { KeybindID.MapEditor_Reset_Rotation, InputCategory.MapEditor },
        { KeybindID.MapEditor_Scramble, InputCategory.MapEditor },
        { KeybindID.MapEditor_Replicate, InputCategory.MapEditor },
        { KeybindID.MapEditor_Cycle_Render_Type, InputCategory.MapEditor },
        { KeybindID.MapEditor_Make_Dummy_Object, InputCategory.MapEditor },
        { KeybindID.MapEditor_Make_Normal_Object, InputCategory.MapEditor },
        { KeybindID.MapEditor_Enable_Game_Presence, InputCategory.MapEditor },
        { KeybindID.MapEditor_Disable_Game_Presence, InputCategory.MapEditor },
        { KeybindID.MapEditor_Visibility_Flip, InputCategory.MapEditor },
        { KeybindID.MapEditor_Visibility_Enable, InputCategory.MapEditor },
        { KeybindID.MapEditor_Visibility_Disable, InputCategory.MapEditor },
        { KeybindID.MapEditor_Global_Visibility_Flip, InputCategory.MapEditor },
        { KeybindID.MapEditor_Global_Visibility_Enable, InputCategory.MapEditor },
        { KeybindID.MapEditor_Global_Visibility_Disable, InputCategory.MapEditor },
        { KeybindID.MapEditor_Configure_Grid_Placement, InputCategory.MapEditor },
        { KeybindID.MapEditor_Cycle_Selected_Grid_Type, InputCategory.MapEditor },
        { KeybindID.MapEditor_Move_to_Primary_Grid, InputCategory.MapEditor },
        { KeybindID.MapEditor_Move_to_Secondary_Grid, InputCategory.MapEditor },
        { KeybindID.MapEditor_Move_to_Tertiary_Grid, InputCategory.MapEditor },
        { KeybindID.MapEditor_SelectAll_Configurable, InputCategory.MapEditor },
        { KeybindID.MapEditor_SelectAll_ObjectType, InputCategory.MapEditor },
        { KeybindID.MapEditor_SelectAll_ModelName, InputCategory.MapEditor },
        { KeybindID.MapEditor_SelectAll_Ceremony, InputCategory.MapEditor },
        { KeybindID.MapEditor_Create_Selection_Group, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Group_0, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Group_1, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Group_2, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Group_3, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Group_4, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Group_5, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Group_6, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Group_7, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Group_8, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Group_9, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Group_10, InputCategory.MapEditor },
        { KeybindID.MapEditor_Rotation_Increment_Cycle_Type, InputCategory.MapEditor },
        { KeybindID.MapEditor_Rotation_Increment_Cycle_Type_Backwards, InputCategory.MapEditor },
        { KeybindID.MapEditor_Position_Increment_Cycle_Type, InputCategory.MapEditor },
        { KeybindID.MapEditor_Position_Increment_Cycle_Type_Backwards, InputCategory.MapEditor },
        { KeybindID.MapEditor_Position_Increment_Toggle_Discrete_Mode, InputCategory.MapEditor },
        { KeybindID.MapEditor_Position_Increment_Positive_X, InputCategory.MapEditor },
        { KeybindID.MapEditor_Position_Increment_Negative_X, InputCategory.MapEditor },
        { KeybindID.MapEditor_Position_Increment_Positive_Y, InputCategory.MapEditor },
        { KeybindID.MapEditor_Position_Increment_Negative_Y, InputCategory.MapEditor },
        { KeybindID.MapEditor_Position_Increment_Positive_Z, InputCategory.MapEditor },
        { KeybindID.MapEditor_Position_Increment_Negative_Z, InputCategory.MapEditor },
        { KeybindID.MapEditor_Toggle_Patrol_Route_Visuals, InputCategory.MapEditor },
        { KeybindID.MapEditor_View_Display_Group, InputCategory.MapEditor },
        { KeybindID.MapEditor_View_Draw_Group, InputCategory.MapEditor },
        { KeybindID.MapEditor_Apply_Display_Group, InputCategory.MapEditor },
        { KeybindID.MapEditor_Apply_Draw_Group, InputCategory.MapEditor },
        { KeybindID.MapEditor_Hide_All_Display_Groups, InputCategory.MapEditor },
        { KeybindID.MapEditor_Show_All_Display_Groups, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Display_Group_Highlights, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Collision_References, InputCategory.MapEditor },
        { KeybindID.MapEditor_Select_Referenced_Collision, InputCategory.MapEditor },
        { KeybindID.MapEditor_Deselect_All, InputCategory.MapEditor },

        // Model Editor

        // Param Editor
        { KeybindID.ParamEditor_Focus_Searchbar, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_Apply_Mass_Edit, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_View_Mass_Edit, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_Import_CSV, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_Export_CSV, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_Export_CSV_Names, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_Export_CSV_Param, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_Export_CSV_All_Rows, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_Export_CSV_Modified_Rows, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_Export_CSV_Selected_Rows, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_Reload_All_Params, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_Reload_Selected_Param, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_RowList_Sort_Rows, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_RowList_Jump_to_Row_ID, InputCategory.ParamEditor },
        { KeybindID.ParamEditor_RowList_Inherit_Referenced_Row_Name, InputCategory.ParamEditor },

        // Text Editor
        { KeybindID.TextEditor_Focus_Searchbar, InputCategory.TextEditor },
        { KeybindID.TextEditor_Configurable_Duplicate, InputCategory.TextEditor },
        { KeybindID.TextEditor_Create_New_Entry, InputCategory.TextEditor },

        // Gparam Editor
        { KeybindID.GparamEditor_Execute_Quick_Edit, InputCategory.GparamEditor },
        { KeybindID.GparamEditor_Generate_Quick_Edit, InputCategory.GparamEditor },
        { KeybindID.GparamEditor_Clear_Quick_Edit, InputCategory.GparamEditor },

        // Material Editor

        // Texture Viewer
        { KeybindID.TextureViewer_Export_Texture, InputCategory.TextureViewer },
        { KeybindID.TextureViewer_Reset_Zoom_Level, InputCategory.TextureViewer },
    };
}

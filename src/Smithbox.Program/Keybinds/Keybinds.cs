using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Keybinds;

#region Keybind IDs
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
#endregion


#region Default Keybinds
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

        InputManager.Bind(KeybindID.Cycle_Gizmo_Translation_Mode, new() { Key = Key.W, Ctrl = true });
        InputManager.Bind(KeybindID.Cycle_Gizmo_Rotation_Mode, new() { Key = Key.E, Ctrl = true });
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

        InputManager.Bind(KeybindID.MapEditor_Rotate_X_Axis, new() { Key = Key.R, Ctrl = true });
        InputManager.Bind(KeybindID.MapEditor_Rotate_Y_Axis, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Rotate_Pivot_Y_Axis, new() { Key = Key.R, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Rotate_Minus_X_Axis, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Rotate_Minus_Y_Axis, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Rotate_Minus_Pivot_Y_Axis, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Rotate_Fixed_Angle, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Reset_Rotation, new() { Key = Key.R, Shift = true });

        InputManager.Bind(KeybindID.MapEditor_Scramble, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Replicate, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Cycle_Render_Type, new() { Key = Key.O });

        InputManager.Bind(KeybindID.MapEditor_Make_Dummy_Object, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Make_Normal_Object, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Enable_Game_Presence, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Disable_Game_Presence, new() { Key = Key.Unknown });

        InputManager.Bind(KeybindID.MapEditor_Visibility_Flip, new() { Key = Key.Y, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Visibility_Enable, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Visibility_Disable, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Global_Visibility_Flip, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Global_Visibility_Enable, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Global_Visibility_Disable, new() { Key = Key.Unknown });

        InputManager.Bind(KeybindID.MapEditor_Configure_Grid_Placement, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Cycle_Selected_Grid_Type, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_Move_to_Primary_Grid, new() { Key = Key.X, Alt = true });
        InputManager.Bind(KeybindID.MapEditor_Move_to_Secondary_Grid, new() { Key = Key.X, Ctrl = true });
        InputManager.Bind(KeybindID.MapEditor_Move_to_Tertiary_Grid, new() { Key = Key.X, Shift = true });

        InputManager.Bind(KeybindID.MapEditor_SelectAll_Configurable, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_SelectAll_ObjectType, new() { Key = Key.Unknown });
        InputManager.Bind(KeybindID.MapEditor_SelectAll_ModelName, new() { Key = Key.Unknown });

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
#endregion
using SoulsFormats.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Keybinds;

public static class InputDefaultBindings
{
    public static void CreateDefaultBindings()
    {
        // Viewport
        InputManager.Bind(InputAction.MoveForward, new() { Key = Key.W });
        InputManager.Bind(InputAction.MoveBackward, new() { Key = Key.S });
        InputManager.Bind(InputAction.MoveLeft, new() { Key = Key.A });
        InputManager.Bind(InputAction.MoveRight, new() { Key = Key.D });
        InputManager.Bind(InputAction.MoveUp, new() { Key = Key.Q });
        InputManager.Bind(InputAction.MoveDown, new() { Key = Key.E });

        InputManager.Bind(InputAction.Frame, new() { Key = Key.F });
        InputManager.Bind(InputAction.Pull, new() { Key = Key.X });
        InputManager.Bind(InputAction.Reset, new() { Key = Key.R });

        InputManager.Bind(InputAction.Cycle_Gizmo_Translation_Mode, new() { Key = Key.W, Ctrl = true });
        InputManager.Bind(InputAction.Cycle_Gizmo_Rotation_Mode, new() { Key = Key.E, Ctrl = true });
        InputManager.Bind(InputAction.Cycle_Gizmo_Origin_Mode, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.Cycle_Gizmo_Space_Mode, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.Cycle_Render_Outline_Mode, new() { Key = Key.Number1, Alt = true });

        // Common
        InputManager.Bind(InputAction.Save, new() { Key = Key.S, Ctrl = true });
        InputManager.Bind(InputAction.Undo, new() { Key = Key.Z, Ctrl = true });
        InputManager.Bind(InputAction.Redo, new() { Key = Key.Y, Ctrl = true });
        InputManager.Bind(InputAction.Undo_Repeat, new() { Key = Key.Z, Ctrl = true, Alt = true });
        InputManager.Bind(InputAction.Redo_Repeat, new() { Key = Key.Y, Ctrl = true, Alt = true });
        InputManager.Bind(InputAction.Up, new() { Key = Key.Up });
        InputManager.Bind(InputAction.Down, new() { Key = Key.Down });
        InputManager.Bind(InputAction.Left, new() { Key = Key.Left });
        InputManager.Bind(InputAction.Right, new() { Key = Key.Right });

        // Contextual
        InputManager.Bind(InputAction.SelectAll, new() { Key = Key.A, Ctrl = true });
        InputManager.Bind(InputAction.Copy, new() { Key = Key.C, Ctrl = true });
        InputManager.Bind(InputAction.Paste, new() { Key = Key.V, Ctrl = true });
        InputManager.Bind(InputAction.Duplicate, new() { Key = Key.D, Ctrl = true });
        InputManager.Bind(InputAction.Delete, new() { Key = Key.Delete, Ctrl = true });
        InputManager.Bind(InputAction.Jump, new() { Key = Key.Q, Ctrl = true });

        InputManager.Bind(InputAction.Reorder_Up, new() { Key = Key.Up, Alt = true });
        InputManager.Bind(InputAction.Reorder_Down, new() { Key = Key.Down, Alt = true });
        InputManager.Bind(InputAction.Reorder_Top, new() { Key = Key.Up, Ctrl = true, Alt = true });
        InputManager.Bind(InputAction.Reorder_Bottom, new() { Key = Key.Down, Ctrl = true, Alt = true });

        InputManager.Bind(InputAction.Apply_to_All, new() { Key = Key.A, Shift = true });

        // Map Editor
        InputManager.Bind(InputAction.MapEditor_Toggle_World_Map_Menu, new() { Key = Key.M });
        InputManager.Bind(InputAction.MapEditor_Reset_World_Map_Zoom_Level, new() { Key = Key.R });

        InputManager.Bind(InputAction.MapEditor_Create_Map_Object, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Duplicate_To_Map, new() { Key = Key.D, Alt = true });

        InputManager.Bind(InputAction.MapEditor_Rotate_X_Axis, new() { Key = Key.R, Ctrl = true });
        InputManager.Bind(InputAction.MapEditor_Rotate_Y_Axis, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Rotate_Pivot_Y_Axis, new() { Key = Key.R, Alt = true });
        InputManager.Bind(InputAction.MapEditor_Rotate_Minus_X_Axis, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Rotate_Minus_Y_Axis, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Rotate_Minus_Pivot_Y_Axis, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Rotate_Fixed_Angle, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Reset_Rotation, new() { Key = Key.R, Shift = true });

        InputManager.Bind(InputAction.MapEditor_Scramble, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Replicate, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Cycle_Render_Type, new() { Key = Key.O });

        InputManager.Bind(InputAction.MapEditor_Make_Dummy_Object, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Make_Normal_Object, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Enable_Game_Presence, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Disable_Game_Presence, new() { Key = Key.Unknown });

        InputManager.Bind(InputAction.MapEditor_Visibility_Flip, new() { Key = Key.Y, Alt = true});
        InputManager.Bind(InputAction.MapEditor_Visibility_Enable, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Visibility_Disable, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Global_Visibility_Flip, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Global_Visibility_Enable, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Global_Visibility_Disable, new() { Key = Key.Unknown });

        InputManager.Bind(InputAction.MapEditor_Configure_Grid_Placement, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Cycle_Selected_Grid_Type, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Move_to_Primary_Grid, new() { Key = Key.X, Alt = true });
        InputManager.Bind(InputAction.MapEditor_Move_to_Secondary_Grid, new() { Key = Key.X, Ctrl = true });
        InputManager.Bind(InputAction.MapEditor_Move_to_Tertiary_Grid, new() { Key = Key.X, Shift = true });

        InputManager.Bind(InputAction.MapEditor_SelectAll_Configurable, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_SelectAll_ObjectType, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_SelectAll_ModelName, new() { Key = Key.Unknown });

        InputManager.Bind(InputAction.MapEditor_Create_Selection_Group, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Select_Group_0, new() { Key = Key.Number0, Alt = true });
        InputManager.Bind(InputAction.MapEditor_Select_Group_1, new() { Key = Key.Number1, Alt = true });
        InputManager.Bind(InputAction.MapEditor_Select_Group_2, new() { Key = Key.Number2, Alt = true });
        InputManager.Bind(InputAction.MapEditor_Select_Group_3, new() { Key = Key.Number3, Alt = true });
        InputManager.Bind(InputAction.MapEditor_Select_Group_4, new() { Key = Key.Number4, Alt = true });
        InputManager.Bind(InputAction.MapEditor_Select_Group_5, new() { Key = Key.Number5, Alt = true });
        InputManager.Bind(InputAction.MapEditor_Select_Group_6, new() { Key = Key.Unknown, Alt = true });
        InputManager.Bind(InputAction.MapEditor_Select_Group_7, new() { Key = Key.Unknown, Alt = true });
        InputManager.Bind(InputAction.MapEditor_Select_Group_8, new() { Key = Key.Unknown, Alt = true });
        InputManager.Bind(InputAction.MapEditor_Select_Group_9, new() { Key = Key.Unknown, Alt = true });
        InputManager.Bind(InputAction.MapEditor_Select_Group_10, new() { Key = Key.Unknown, Alt = true });

        InputManager.Bind(InputAction.MapEditor_Rotation_Increment_Cycle_Type, new() { Key = Key.F2 });
        InputManager.Bind(InputAction.MapEditor_Rotation_Increment_Cycle_Type_Backwards, new() { Key = Key.Unknown });

        InputManager.Bind(InputAction.MapEditor_Position_Increment_Cycle_Type, new() { Key = Key.F1 });
        InputManager.Bind(InputAction.MapEditor_Position_Increment_Toggle_Discrete_Mode, new() { Key = Key.F3 });
        InputManager.Bind(InputAction.MapEditor_Position_Increment_Positive_X, new() { Key = Key.Right, Ctrl = true });
        InputManager.Bind(InputAction.MapEditor_Position_Increment_Negative_X, new() { Key = Key.Left, Ctrl = true });
        InputManager.Bind(InputAction.MapEditor_Position_Increment_Positive_Y, new() { Key = Key.Up, Ctrl = true });
        InputManager.Bind(InputAction.MapEditor_Position_Increment_Negative_Y, new() { Key = Key.Down, Ctrl = true });
        InputManager.Bind(InputAction.MapEditor_Position_Increment_Positive_Z, new() { Key = Key.Up, Alt = true });
        InputManager.Bind(InputAction.MapEditor_Position_Increment_Negative_Z, new() { Key = Key.Down, Alt = true });

        InputManager.Bind(InputAction.MapEditor_Toggle_Patrol_Route_Visuals, new() { Key = Key.Unknown });

        InputManager.Bind(InputAction.MapEditor_View_Display_Group, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_View_Draw_Group, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Apply_Display_Group, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Apply_Draw_Group, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Hide_All_Display_Groups, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Show_All_Display_Groups, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.MapEditor_Select_Display_Group_Highlights, new() { Key = Key.Unknown });

        // Model Editor

        // Param Editor
        InputManager.Bind(InputAction.ParamEditor_Focus_Searchbar, new() { Key = Key.F, Ctrl = true });

        InputManager.Bind(InputAction.ParamEditor_Apply_Mass_Edit, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.ParamEditor_View_Mass_Edit, new() { Key = Key.Unknown });

        InputManager.Bind(InputAction.ParamEditor_Import_CSV, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.ParamEditor_Export_CSV, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.ParamEditor_Export_CSV_Names, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.ParamEditor_Export_CSV_Param, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.ParamEditor_Export_CSV_All_Rows, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.ParamEditor_Export_CSV_Modified_Rows, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.ParamEditor_Export_CSV_Selected_Rows, new() { Key = Key.Unknown });

        InputManager.Bind(InputAction.ParamEditor_Reload_All_Params, new() { Key = Key.F6 });
        InputManager.Bind(InputAction.ParamEditor_Reload_Selected_Param, new() { Key = Key.F5 });

        InputManager.Bind(InputAction.ParamEditor_RowList_Sort_Rows, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.ParamEditor_RowList_Jump_to_Row_ID, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.ParamEditor_RowList_Inherit_Referenced_Row_Name, new() { Key = Key.Unknown });

        // Text Editor
        InputManager.Bind(InputAction.TextEditor_Focus_Searchbar, new() { Key = Key.F, Ctrl = true });
        InputManager.Bind(InputAction.TextEditor_Configurable_Duplicate, new() { Key = Key.D, Alt = true });
        InputManager.Bind(InputAction.TextEditor_Create_New_Entry, new() { Key = Key.Insert });

        // Gparam Editor
        InputManager.Bind(InputAction.GparamEditor_Execute_Quick_Edit, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.GparamEditor_Generate_Quick_Edit, new() { Key = Key.Unknown });
        InputManager.Bind(InputAction.GparamEditor_Clear_Quick_Edit, new() { Key = Key.Unknown });

        // Material Editor

        // Texture Viewer
        InputManager.Bind(InputAction.TextureViewer_Export_Texture, new() { Key = Key.X, Ctrl = true });
        InputManager.Bind(InputAction.TextureViewer_Reset_Zoom_Level, new() { Key = Key.R, Ctrl = true });
    }
}
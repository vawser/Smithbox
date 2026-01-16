using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Keybinds;

public static class InputActionMetadata
{
    public static readonly Dictionary<InputAction, InputCategory> Category
        = new()
    {
        // Viewport
        { InputAction.MoveForward, InputCategory.Viewport },
        { InputAction.MoveBackward, InputCategory.Viewport },
        { InputAction.MoveLeft, InputCategory.Viewport },
        { InputAction.MoveRight, InputCategory.Viewport },
        { InputAction.MoveUp, InputCategory.Viewport },
        { InputAction.MoveDown, InputCategory.Viewport },
        { InputAction.Frame, InputCategory.Viewport },
        { InputAction.Pull, InputCategory.Viewport },
        { InputAction.Reset, InputCategory.Viewport },
        { InputAction.Cycle_Gizmo_Translation_Mode, InputCategory.Viewport },
        { InputAction.Cycle_Gizmo_Rotation_Mode, InputCategory.Viewport },
        { InputAction.Cycle_Gizmo_Origin_Mode, InputCategory.Viewport },
        { InputAction.Cycle_Gizmo_Space_Mode, InputCategory.Viewport },
        { InputAction.Cycle_Render_Outline_Mode, InputCategory.Viewport },

        // Common
        { InputAction.Save, InputCategory.Common },
        { InputAction.Undo, InputCategory.Common },
        { InputAction.Redo, InputCategory.Common },
        { InputAction.Undo_Repeat, InputCategory.Common },
        { InputAction.Redo_Repeat, InputCategory.Common },
        { InputAction.Up, InputCategory.Common },
        { InputAction.Down, InputCategory.Common },
        { InputAction.Left, InputCategory.Common },
        { InputAction.Right, InputCategory.Common },

        // Contextual
        { InputAction.SelectAll, InputCategory.Contextual },
        { InputAction.Copy, InputCategory.Contextual },
        { InputAction.Paste, InputCategory.Contextual },
        { InputAction.Duplicate, InputCategory.Contextual },
        { InputAction.Delete, InputCategory.Contextual },
        { InputAction.Jump, InputCategory.Contextual },
        { InputAction.Reorder_Up, InputCategory.Contextual },
        { InputAction.Reorder_Down, InputCategory.Contextual },
        { InputAction.Reorder_Top, InputCategory.Contextual },
        { InputAction.Reorder_Bottom, InputCategory.Contextual },
        { InputAction.Apply_to_All, InputCategory.Contextual },

        // Map 
        { InputAction.MapEditor_Toggle_World_Map_Menu, InputCategory.MapEditor },
        { InputAction.MapEditor_Reset_World_Map_Zoom_Level, InputCategory.MapEditor },
        { InputAction.MapEditor_Create_Map_Object, InputCategory.MapEditor },
        { InputAction.MapEditor_Duplicate_To_Map, InputCategory.MapEditor },
        { InputAction.MapEditor_Rotate_X_Axis, InputCategory.MapEditor },
        { InputAction.MapEditor_Rotate_Y_Axis, InputCategory.MapEditor },
        { InputAction.MapEditor_Rotate_Pivot_Y_Axis, InputCategory.MapEditor },
        { InputAction.MapEditor_Rotate_Minus_X_Axis, InputCategory.MapEditor },
        { InputAction.MapEditor_Rotate_Minus_Y_Axis, InputCategory.MapEditor },
        { InputAction.MapEditor_Rotate_Minus_Pivot_Y_Axis, InputCategory.MapEditor },
        { InputAction.MapEditor_Rotate_Fixed_Angle, InputCategory.MapEditor },
        { InputAction.MapEditor_Reset_Rotation, InputCategory.MapEditor },
        { InputAction.MapEditor_Scramble, InputCategory.MapEditor },
        { InputAction.MapEditor_Replicate, InputCategory.MapEditor },
        { InputAction.MapEditor_Cycle_Render_Type, InputCategory.MapEditor },
        { InputAction.MapEditor_Make_Dummy_Object, InputCategory.MapEditor },
        { InputAction.MapEditor_Make_Normal_Object, InputCategory.MapEditor },
        { InputAction.MapEditor_Enable_Game_Presence, InputCategory.MapEditor },
        { InputAction.MapEditor_Disable_Game_Presence, InputCategory.MapEditor },
        { InputAction.MapEditor_Visibility_Flip, InputCategory.MapEditor },
        { InputAction.MapEditor_Visibility_Enable, InputCategory.MapEditor },
        { InputAction.MapEditor_Visibility_Disable, InputCategory.MapEditor },
        { InputAction.MapEditor_Global_Visibility_Flip, InputCategory.MapEditor },
        { InputAction.MapEditor_Global_Visibility_Enable, InputCategory.MapEditor },
        { InputAction.MapEditor_Global_Visibility_Disable, InputCategory.MapEditor },
        { InputAction.MapEditor_Configure_Grid_Placement, InputCategory.MapEditor },
        { InputAction.MapEditor_Cycle_Selected_Grid_Type, InputCategory.MapEditor },
        { InputAction.MapEditor_Move_to_Primary_Grid, InputCategory.MapEditor },
        { InputAction.MapEditor_Move_to_Secondary_Grid, InputCategory.MapEditor },
        { InputAction.MapEditor_Move_to_Tertiary_Grid, InputCategory.MapEditor },
        { InputAction.MapEditor_SelectAll_Configurable, InputCategory.MapEditor },
        { InputAction.MapEditor_SelectAll_ObjectType, InputCategory.MapEditor },
        { InputAction.MapEditor_SelectAll_ModelName, InputCategory.MapEditor },
        { InputAction.MapEditor_Create_Selection_Group, InputCategory.MapEditor },
        { InputAction.MapEditor_Select_Group_0, InputCategory.MapEditor },
        { InputAction.MapEditor_Select_Group_1, InputCategory.MapEditor },
        { InputAction.MapEditor_Select_Group_2, InputCategory.MapEditor },
        { InputAction.MapEditor_Select_Group_3, InputCategory.MapEditor },
        { InputAction.MapEditor_Select_Group_4, InputCategory.MapEditor },
        { InputAction.MapEditor_Select_Group_5, InputCategory.MapEditor },
        { InputAction.MapEditor_Select_Group_6, InputCategory.MapEditor },
        { InputAction.MapEditor_Select_Group_7, InputCategory.MapEditor },
        { InputAction.MapEditor_Select_Group_8, InputCategory.MapEditor },
        { InputAction.MapEditor_Select_Group_9, InputCategory.MapEditor },
        { InputAction.MapEditor_Select_Group_10, InputCategory.MapEditor },
        { InputAction.MapEditor_Rotation_Increment_Cycle_Type, InputCategory.MapEditor },
        { InputAction.MapEditor_Rotation_Increment_Cycle_Type_Backwards, InputCategory.MapEditor },
        { InputAction.MapEditor_Position_Increment_Cycle_Type, InputCategory.MapEditor },
        { InputAction.MapEditor_Position_Increment_Cycle_Type_Backwards, InputCategory.MapEditor },
        { InputAction.MapEditor_Position_Increment_Toggle_Discrete_Mode, InputCategory.MapEditor },
        { InputAction.MapEditor_Position_Increment_Positive_X, InputCategory.MapEditor },
        { InputAction.MapEditor_Position_Increment_Negative_X, InputCategory.MapEditor },
        { InputAction.MapEditor_Position_Increment_Positive_Y, InputCategory.MapEditor },
        { InputAction.MapEditor_Position_Increment_Negative_Y, InputCategory.MapEditor },
        { InputAction.MapEditor_Position_Increment_Positive_Z, InputCategory.MapEditor },
        { InputAction.MapEditor_Position_Increment_Negative_Z, InputCategory.MapEditor },
        { InputAction.MapEditor_Toggle_Patrol_Route_Visuals, InputCategory.MapEditor },
        { InputAction.MapEditor_View_Display_Group, InputCategory.MapEditor },
        { InputAction.MapEditor_View_Draw_Group, InputCategory.MapEditor },
        { InputAction.MapEditor_Apply_Display_Group, InputCategory.MapEditor },
        { InputAction.MapEditor_Apply_Draw_Group, InputCategory.MapEditor },
        { InputAction.MapEditor_Hide_All_Display_Groups, InputCategory.MapEditor },
        { InputAction.MapEditor_Show_All_Display_Groups, InputCategory.MapEditor },
        { InputAction.MapEditor_Select_Display_Group_Highlights, InputCategory.MapEditor },

        // Model Editor

        // Param Editor
        { InputAction.ParamEditor_Focus_Searchbar, InputCategory.ParamEditor },
        { InputAction.ParamEditor_Apply_Mass_Edit, InputCategory.ParamEditor },
        { InputAction.ParamEditor_View_Mass_Edit, InputCategory.ParamEditor },
        { InputAction.ParamEditor_Import_CSV, InputCategory.ParamEditor },
        { InputAction.ParamEditor_Export_CSV, InputCategory.ParamEditor },
        { InputAction.ParamEditor_Export_CSV_Names, InputCategory.ParamEditor },
        { InputAction.ParamEditor_Export_CSV_Param, InputCategory.ParamEditor },
        { InputAction.ParamEditor_Export_CSV_All_Rows, InputCategory.ParamEditor },
        { InputAction.ParamEditor_Export_CSV_Modified_Rows, InputCategory.ParamEditor },
        { InputAction.ParamEditor_Export_CSV_Selected_Rows, InputCategory.ParamEditor },
        { InputAction.ParamEditor_Reload_All_Params, InputCategory.ParamEditor },
        { InputAction.ParamEditor_Reload_Selected_Param, InputCategory.ParamEditor },
        { InputAction.ParamEditor_RowList_Sort_Rows, InputCategory.ParamEditor },
        { InputAction.ParamEditor_RowList_Jump_to_Row_ID, InputCategory.ParamEditor },
        { InputAction.ParamEditor_RowList_Inherit_Referenced_Row_Name, InputCategory.ParamEditor },

        // Text Editor
        { InputAction.TextEditor_Focus_Searchbar, InputCategory.TextEditor },
        { InputAction.TextEditor_Configurable_Duplicate, InputCategory.TextEditor },
        { InputAction.TextEditor_Create_New_Entry, InputCategory.TextEditor },

        // Gparam Editor
        { InputAction.GparamEditor_Execute_Quick_Edit, InputCategory.GparamEditor },
        { InputAction.GparamEditor_Generate_Quick_Edit, InputCategory.GparamEditor },
        { InputAction.GparamEditor_Clear_Quick_Edit, InputCategory.GparamEditor },

        // Material Editor

        // Texture Viewer
        { InputAction.TextureViewer_Export_Texture, InputCategory.TextureViewer },
        { InputAction.TextureViewer_Reset_Zoom_Level, InputCategory.TextureViewer },
    };
}
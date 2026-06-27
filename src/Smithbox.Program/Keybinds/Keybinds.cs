using System.ComponentModel.DataAnnotations;
using Veldrid;

namespace StudioCore.Keybinds;

public enum KeybindID
{
    // Viewport
    [Display(Name = "KEY_Move_Forward", Description = "KEY_Move_Forward_TT")]
    MoveForward,

    [Display(Name = "KEY_Move_Backward", Description = "KEY_Move_Backward_TT")]
    MoveBackward,

    [Display(Name = "KEY_Move_Left", Description = "KEY_Move_Left_TT")]
    MoveLeft,

    [Display(Name = "KEY_Move_Right", Description = "KEY_Move_Right_TT")]
    MoveRight,

    [Display(Name = "KEY_Move_Up", Description = "KEY_Move_Up_TT")]
    MoveUp,

    [Display(Name = "KEY_Move_Down", Description = "KEY_Move_Down_TT")]
    MoveDown,

    [Display(Name = "KEY_Frame", Description = "KEY_Frame_TT")]
    Frame,

    [Display(Name = "KEY_Pull", Description = "KEY_Pull_TT")]
    Pull,

    [Display(Name = "KEY_Reset", Description = "KEY_Reset_TT")]
    Reset,

    [Display(Name = "KEY_Cycle_Gizmo_Translation_Mode", Description = "KEY_Cycle_Gizmo_Translation_Mode_TT")]
    Cycle_Gizmo_Translation_Mode,

    [Display(Name = "KEY_Cycle_Gizmo_Rotation_Mode", Description = "KEY_Cycle_Gizmo_Rotation_Mode_TT")]
    Cycle_Gizmo_Rotation_Mode,

    [Display(Name = "KEY_Cycle_Gizmo_Scale_Mode", Description = "KEY_Cycle_Gizmo_Scale_Mode_TT")]
    Cycle_Gizmo_Scale_Mode,

    [Display(Name = "KEY_Cycle_Gizmo_Origin_Mode", Description = "KEY_Cycle_Gizmo_Origin_Mode_TT")]
    Cycle_Gizmo_Origin_Mode,

    [Display(Name = "KEY_Cycle_Gizmo_Space_Mode", Description = "KEY_Cycle_Gizmo_Space_Mode_TT")]
    Cycle_Gizmo_Space_Mode,

    [Display(Name = "KEY_Cycle_Render_Outline_Mode", Description = "KEY_Cycle_Render_Outline_Mode_TT")]
    Cycle_Render_Outline_Mode,

    // Common Actions
    [Display(Name = "KEY_Save", Description = "KEY_Save_TT")]
    Save,

    [Display(Name = "KEY_Undo", Description = "KEY_Undo_TT")]
    Undo,

    [Display(Name = "KEY_Redo", Description = "KEY_Redo_TT")]
    Redo,

    [Display(Name = "KEY_Undo_Repeat", Description = "KEY_Undo_Repeat_TT")]
    Undo_Repeat,

    [Display(Name = "KEY_Redo_Repeat", Description = "KEY_Redo_Repeat_TT")]
    Redo_Repeat,

    [Display(Name = "KEY_Up", Description = "KEY_Up_TT")]
    Up,

    [Display(Name = "KEY_Down", Description = "KEY_Down_TT")]
    Down,

    [Display(Name = "KEY_Left", Description = "KEY_Left_TT")]
    Left,

    [Display(Name = "KEY_Right", Description = "KEY_Right_TT")]
    Right,

    // Contextual Actions
    [Display(Name = "KEY_Add", Description = "KEY_Add_TT")]
    Add,

    [Display(Name = "KEY_Select_All", Description = "KEY_Select_All_TT")]
    SelectAll,

    [Display(Name = "KEY_Copy", Description = "KEY_Copy_TT")]
    Copy,

    [Display(Name = "KEY_Paste", Description = "KEY_Paste_TT")]
    Paste,

    [Display(Name = "KEY_Duplicate", Description = "KEY_Duplicate_TT")]
    Duplicate,

    [Display(Name = "KEY_Delete", Description = "KEY_Delete_TT")]
    Delete,

    [Display(Name = "KEY_Jump", Description = "KEY_Jump_TT")]
    Jump,

    [Display(Name = "KEY_Reorder_Up", Description = "KEY_Reorder_Up_TT")]
    Reorder_Up,

    [Display(Name = "KEY_Reorder_Down", Description = "KEY_Reorder_Down_TT")]
    Reorder_Down,

    [Display(Name = "KEY_Reorder_Top", Description = "KEY_Reorder_Top_TT")]
    Reorder_Top,

    [Display(Name = "KEY_Reorder_Bottom", Description = "KEY_Reorder_Bottom_TT")]
    Reorder_Bottom,

    [Display(Name = "KEY_Apply_to_All", Description = "KEY_Apply_to_All_TT")]
    Apply_to_All,

    [Display(Name = "KEY_Toggle_Tools_Menu", Description = "KEY_Toggle_Tools_Menu_TT")]
    Toggle_Tools_Menu,

    // Map Editor
    [Display(Name = "KEY_MapEditor_Toggle_World_Map_Menu", Description = "KEY_MapEditor_Toggle_World_Map_Menu_TT")]
    MapEditor_Toggle_World_Map_Menu,

    [Display(Name = "KEY_MapEditor_Reset_World_Map_Zoom_Level", Description = "KEY_MapEditor_Reset_World_Map_Zoom_Level_TT")]
    MapEditor_Reset_World_Map_Zoom_Level,

    [Display(Name = "KEY_MapEditor_Create_Map_Object", Description = "KEY_MapEditor_Create_Map_Object_TT")]
    MapEditor_Create_Map_Object,

    [Display(Name = "KEY_MapEditor_Duplicate_To_Map", Description = "KEY_MapEditor_Duplicate_To_Map_TT")]
    MapEditor_Duplicate_To_Map,

    [Display(Name = "KEY_MapEditor_Rotate_X_Axis", Description = "KEY_MapEditor_Rotate_X_Axis_TT")]
    MapEditor_Rotate_X_Axis,

    [Display(Name = "KEY_MapEditor_Rotate_Y_Axis", Description = "KEY_MapEditor_Rotate_Y_Axis_TT")]
    MapEditor_Rotate_Y_Axis,

    [Display(Name = "KEY_MapEditor_Rotate_Pivot_Y_Axis", Description = "KEY_MapEditor_Rotate_Pivot_Y_Axis_TT")]
    MapEditor_Rotate_Pivot_Y_Axis,

    [Display(Name = "KEY_MapEditor_Rotate_Minus_X_Axis", Description = "KEY_MapEditor_Rotate_Minus_X_Axis_TT")]
    MapEditor_Rotate_Minus_X_Axis,

    [Display(Name = "KEY_MapEditor_Rotate_Minus_Y_Axis", Description = "KEY_MapEditor_Rotate_Minus_Y_Axis_TT")]
    MapEditor_Rotate_Minus_Y_Axis,

    [Display(Name = "KEY_MapEditor_Rotate_Minus_Pivot_Y_Axis", Description = "KEY_MapEditor_Rotate_Minus_Pivot_Y_Axis_TT")]
    MapEditor_Rotate_Minus_Pivot_Y_Axis,

    [Display(Name = "KEY_MapEditor_Rotate_Fixed_Angle", Description = "KEY_MapEditor_Rotate_Fixed_Angle_TT")]
    MapEditor_Rotate_Fixed_Angle,

    [Display(Name = "KEY_MapEditor_Reset_Rotation", Description = "KEY_MapEditor_Reset_Rotation_TT")]
    MapEditor_Reset_Rotation,

    [Display(Name = "KEY_MapEditor_Scramble", Description = "KEY_MapEditor_Scramble_TT")]
    MapEditor_Scramble,

    [Display(Name = "KEY_MapEditor_Replicate", Description = "KEY_MapEditor_Replicate_TT")]
    MapEditor_Replicate,

    [Display(Name = "KEY_MapEditor_Cycle_Render_Type", Description = "KEY_MapEditor_Cycle_Render_Type_TT")]
    MapEditor_Cycle_Render_Type,

    [Display(Name = "KEY_MapEditor_Make_Dummy_Object", Description = "KEY_MapEditor_Make_Dummy_Object_TT")]
    MapEditor_Make_Dummy_Object,

    [Display(Name = "KEY_MapEditor_Make_Normal_Object", Description = "KEY_MapEditor_Make_Normal_Object_TT")]
    MapEditor_Make_Normal_Object,

    [Display(Name = "KEY_MapEditor_Enable_Game_Presence_Object", Description = "KEY_MapEditor_Enable_Game_Presence_TT")]
    MapEditor_Enable_Game_Presence,

    [Display(Name = "KEY_MapEditor_Disable_Game_Presence_Object", Description = "KEY_MapEditor_Disable_Game_Presence_TT")]
    MapEditor_Disable_Game_Presence,

    [Display(Name = "KEY_MapEditor_Visibility_Flip", Description = "KEY_MapEditor_Visibility_Flip_TT")]
    MapEditor_Visibility_Flip,

    [Display(Name = "KEY_MapEditor_Visibility_Enable", Description = "KEY_MapEditor_Visibility_Enable_TT")]
    MapEditor_Visibility_Enable,

    [Display(Name = "KEY_MapEditor_Visibility_Disable", Description = "KEY_MapEditor_Visibility_Disable_TT")]
    MapEditor_Visibility_Disable,

    [Display(Name = "KEY_MapEditor_Global_Visibility_Flip", Description = "KEY_MapEditor_Global_Visibility_Flip_TT")]
    MapEditor_Global_Visibility_Flip,

    [Display(Name = "KEY_MapEditor_Global_Visibility_Enable", Description = "KEY_MapEditor_Global_Visibility_Enable_TT")]
    MapEditor_Global_Visibility_Enable,

    [Display(Name = "KEY_MapEditor_Global_Visibility_Disable", Description = "KEY_MapEditor_Global_Visibility_Disable_TT")]
    MapEditor_Global_Visibility_Disable,

    [Display(Name = "KEY_MapEditor_Configure_Grid_Placement", Description = "KEY_MapEditor_Configure_Grid_Placement_TT")]
    MapEditor_Configure_Grid_Placement,

    [Display(Name = "KEY_MapEditor_Cycle_Selected_Grid_Type", Description = "KEY_MapEditor_Cycle_Selected_Grid_Type_TT")]
    MapEditor_Cycle_Selected_Grid_Type,

    [Display(Name = "KEY_MapEditor_Move_to_Primary_Grid", Description = "KEY_MapEditor_Move_to_Primary_Grid_TT")]
    MapEditor_Move_to_Primary_Grid,

    [Display(Name = "KEY_MapEditor_Move_to_Secondary_Grid", Description = "KEY_MapEditor_Move_to_Secondary_Grid_TT")]
    MapEditor_Move_to_Secondary_Grid,

    [Display(Name = "KEY_MapEditor_Move_to_Tertiary_Grid", Description = "KEY_MapEditor_Move_to_Tertiary_Grid_TT")]
    MapEditor_Move_to_Tertiary_Grid,

    [Display(Name = "KEY_MapEditor_SelectAll_Configurable", Description = "KEY_MapEditor_SelectAll_Configurable_TT")]
    MapEditor_SelectAll_Configurable,

    [Display(Name = "KEY_MapEditor_SelectAll_ObjectType", Description = "KEY_MapEditor_SelectAll_ObjectType_TT")]
    MapEditor_SelectAll_ObjectType,

    [Display(Name = "KEY_MapEditor_SelectAll_ModelName", Description = "KEY_MapEditor_SelectAll_ModelName_TT")]
    MapEditor_SelectAll_ModelName,

    [Display(Name = "KEY_MapEditor_SelectAll_Ceremony", Description = "KEY_MapEditor_SelectAll_Ceremony_TT")]
    MapEditor_SelectAll_Ceremony,

    [Display(Name = "KEY_MapEditor_Create_Selection_Group", Description = "KEY_MapEditor_Create_Selection_Group_TT")]
    MapEditor_Create_Selection_Group,

    [Display(Name = "KEY_MapEditor_Select_Group_0", Description = "KEY_MapEditor_Select_Group_0_TT")]
    MapEditor_Select_Group_0,

    [Display(Name = "KEY_MapEditor_Select_Group_1", Description = "KEY_MapEditor_Select_Group_1_TT")]
    MapEditor_Select_Group_1,

    [Display(Name = "KEY_MapEditor_Select_Group_2", Description = "KEY_MapEditor_Select_Group_2_TT")]
    MapEditor_Select_Group_2,

    [Display(Name = "KEY_MapEditor_Select_Group_3", Description = "KEY_MapEditor_Select_Group_3_TT")]
    MapEditor_Select_Group_3,

    [Display(Name = "KEY_MapEditor_Select_Group_4", Description = "KEY_MapEditor_Select_Group_4_TT")]
    MapEditor_Select_Group_4,

    [Display(Name = "KEY_MapEditor_Select_Group_5", Description = "KEY_MapEditor_Select_Group_5_TT")]
    MapEditor_Select_Group_5,

    [Display(Name = "KEY_MapEditor_Select_Group_6", Description = "KEY_MapEditor_Select_Group_6_TT")]
    MapEditor_Select_Group_6,

    [Display(Name = "KEY_MapEditor_Select_Group_7", Description = "KEY_MapEditor_Select_Group_7_TT")]
    MapEditor_Select_Group_7,

    [Display(Name = "KEY_MapEditor_Select_Group_8", Description = "KEY_MapEditor_Select_Group_8_TT")]
    MapEditor_Select_Group_8,

    [Display(Name = "KEY_MapEditor_Select_Group_9", Description = "KEY_MapEditor_Select_Group_9_TT")]
    MapEditor_Select_Group_9,

    [Display(Name = "KEY_MapEditor_Select_Group_10", Description = "KEY_MapEditor_Select_Group_10_TT")]
    MapEditor_Select_Group_10,

    [Display(Name = "KEY_MapEditor_Rotation_Increment_Cycle_Type", Description = "KEY_MapEditor_Rotation_Increment_Cycle_Type_TT")]
    MapEditor_Rotation_Increment_Cycle_Type,

    [Display(Name = "KEY_MapEditor_Rotation_Increment_Cycle_Type_Backwards", Description = "KEY_MapEditor_Rotation_Increment_Cycle_Type_Backwards_TT")]
    MapEditor_Rotation_Increment_Cycle_Type_Backwards,

    [Display(Name = "KEY_MapEditor_Position_Increment_Cycle_Type", Description = "KEY_MapEditor_Position_Increment_Cycle_Type_TT")]
    MapEditor_Position_Increment_Cycle_Type,

    [Display(Name = "KEY_MapEditor_Position_Increment_Cycle_Type_Backwards", Description = "KEY_MapEditor_Position_Increment_Cycle_Type_Backwards_TT")]
    MapEditor_Position_Increment_Cycle_Type_Backwards,

    [Display(Name = "KEY_MapEditor_Position_Increment_Toggle_Discrete_Mode", Description = "KEY_MapEditor_Position_Increment_Toggle_Discrete_Mode_TT")]
    MapEditor_Position_Increment_Toggle_Discrete_Mode,

    [Display(Name = "KEY_MapEditor_Position_Increment_Positive_X", Description = "KEY_MapEditor_Position_Increment_Positive_X_TT")]
    MapEditor_Position_Increment_Positive_X,

    [Display(Name = "KEY_MapEditor_Position_Increment_Negative_X", Description = "KEY_MapEditor_Position_Increment_Negative_X_TT")]
    MapEditor_Position_Increment_Negative_X,

    [Display(Name = "KEY_MapEditor_Position_Increment_Positive_Y", Description = "KEY_MapEditor_Position_Increment_Positive_Y_TT")]
    MapEditor_Position_Increment_Positive_Y,

    [Display(Name = "KEY_MapEditor_Position_Increment_Negative_Y", Description = "KEY_MapEditor_Position_Increment_Negative_Y_TT")]
    MapEditor_Position_Increment_Negative_Y,

    [Display(Name = "KEY_MapEditor_Position_Increment_Positive_Z", Description = "KEY_MapEditor_Position_Increment_Positive_Z_TT")]
    MapEditor_Position_Increment_Positive_Z,

    [Display(Name = "KEY_MapEditor_Position_Increment_Negative_Z", Description = "KEY_MapEditor_Position_Increment_Negative_Z_TT")]
    MapEditor_Position_Increment_Negative_Z,

    [Display(Name = "KEY_MapEditor_Toggle_Patrol_Route_Visuals", Description = "KEY_MapEditor_Toggle_Patrol_Route_Visuals_TT")]
    MapEditor_Toggle_Patrol_Route_Visuals,

    [Display(Name = "KEY_MapEditor_View_Display_Group", Description = "KEY_MapEditor_View_Display_Group_TT")]
    MapEditor_View_Display_Group,

    [Display(Name = "KEY_MapEditor_View_Draw_Group", Description = "KEY_MapEditor_View_Draw_Group_TT")]
    MapEditor_View_Draw_Group,

    [Display(Name = "KEY_MapEditor_Apply_Display_Group", Description = "KEY_MapEditor_Apply_Display_Group_TT")]
    MapEditor_Apply_Display_Group,

    [Display(Name = "KEY_MapEditor_Apply_Draw_Group", Description = "KEY_MapEditor_Apply_Draw_Group_TT")]
    MapEditor_Apply_Draw_Group,

    [Display(Name = "KEY_MapEditor_Hide_All_Display_Groups", Description = "KEY_MapEditor_Hide_All_Display_Groups_TT")]
    MapEditor_Hide_All_Display_Groups,

    [Display(Name = "KEY_MapEditor_Show_All_Display_Groups", Description = "KEY_MapEditor_Show_All_Display_Groups_TT")]
    MapEditor_Show_All_Display_Groups,

    [Display(Name = "KEY_MapEditor_Select_Display_Group_Highlights", Description = "KEY_MapEditor_Select_Display_Group_Highlights_TT")]
    MapEditor_Select_Display_Group_Highlights,

    [Display(Name = "KEY_MapEditor_Select_Collision_References", Description = "KEY_MapEditor_Select_Collision_References_TT")]
    MapEditor_Select_Collision_References,

    [Display(Name = "KEY_MapEditor_Select_Referenced_Collision", Description = "KEY_MapEditor_Select_Referenced_Collision_TT")]
    MapEditor_Select_Referenced_Collision,

    [Display(Name = "KEY_MapEditor_Deselect_All", Description = "KEY_MapEditor_Deselect_All_TT")]
    MapEditor_Deselect_All,

    // Model Editor
    [Display(Name = "KEY_ModelEditor_Select_Primitives_Only", Description = "KEY_ModelEditor_Select_Primitives_Only_TT")]
    ModelEditor_Select_Primitives_Only,

    // Param Editor
    [Display(Name = "KEY_ParamEditor_Focus_Searchbar", Description = "KEY_ParamEditor_Focus_Searchbar_TT")]
    ParamEditor_Focus_Searchbar,

    [Display(Name = "KEY_ParamEditor_Apply_Mass_Edit", Description = "KEY_ParamEditor_Apply_Mass_Edit_TT")]
    ParamEditor_Apply_Mass_Edit,

    [Display(Name = "KEY_ParamEditor_View_Mass_Edit", Description = "KEY_ParamEditor_View_Mass_Edit_TT")]
    ParamEditor_View_Mass_Edit,

    [Display(Name = "KEY_ParamEditor_Import_CSV", Description = "KEY_ParamEditor_Import_CSV_TT")]
    ParamEditor_Import_CSV,

    [Display(Name = "KEY_ParamEditor_Export_CSV", Description = "KEY_ParamEditor_Export_CSV_TT")]
    ParamEditor_Export_CSV,

    [Display(Name = "KEY_ParamEditor_Export_CSV_Names", Description = "KEY_ParamEditor_Export_CSV_Names_TT")]
    ParamEditor_Export_CSV_Names,

    [Display(Name = "KEY_ParamEditor_Export_CSV_Param", Description = "KEY_ParamEditor_Export_CSV_Param_TT")]
    ParamEditor_Export_CSV_Param,

    [Display(Name = "KEY_ParamEditor_Export_CSV_All_Rows", Description = "KEY_ParamEditor_Export_CSV_All_Rows_TT")]
    ParamEditor_Export_CSV_All_Rows,

    [Display(Name = "KEY_ParamEditor_Export_CSV_Modified_Rows", Description = "KEY_ParamEditor_Export_CSV_Modified_Rows_TT")]
    ParamEditor_Export_CSV_Modified_Rows,

    [Display(Name = "KEY_ParamEditor_Export_CSV_Selected_Rows", Description = "KEY_ParamEditor_Export_CSV_Selected_Rows_TT")]
    ParamEditor_Export_CSV_Selected_Rows,

    [Display(Name = "KEY_ParamEditor_Reload_All_Params", Description = "KEY_ParamEditor_Reload_All_Params_TT")]
    ParamEditor_Reload_All_Params,

    [Display(Name = "KEY_ParamEditor_Reload_Selected_Param", Description = "KEY_ParamEditor_Reload_Selected_Param_TT")]
    ParamEditor_Reload_Selected_Param,

    [Display(Name = "KEY_ParamEditor_RowList_Sort_Rows", Description = "KEY_ParamEditor_RowList_Sort_Rows_TT")]
    ParamEditor_RowList_Sort_Rows,

    [Display(Name = "KEY_ParamEditor_RowList_Jump_to_Row_ID", Description = "KEY_ParamEditor_RowList_Jump_to_Row_ID_TT")]
    ParamEditor_RowList_Jump_to_Row_ID,

    [Display(Name = "KEY_ParamEditor_RowList_Inherit_Referenced_Row_Name", Description = "KEY_ParamEditor_RowList_Inherit_Referenced_Row_Name_TT")]
    ParamEditor_RowList_Inherit_Referenced_Row_Name,

    // Text Editor
    [Display(Name = "KEY_TextEditor_Focus_Searchbar", Description = "KEY_TextEditor_Focus_Searchbar_TT")]
    TextEditor_Focus_Searchbar,

    [Display(Name = "KEY_TextEditor_Create_New_Entry", Description = "KEY_TextEditor_Create_New_Entry_TT")]
    TextEditor_Create_New_Entry,

    [Display(Name = "KEY_TextEditor_Configurable_Duplicate", Description = "KEY_TextEditor_Configurable_Duplicate_TT")]
    TextEditor_Configurable_Duplicate,

    // Gparam Editor
    [Display(Name = "KEY_GparamEditor_Add_Missing_Groups", Description = "KEY_GparamEditor_Add_Missing_Groups_TT")]
    GparamEditor_Add_Missing_Groups,

    [Display(Name = "KEY_GparamEditor_Add_Missing_Fields", Description = "KEY_GparamEditor_Add_Missing_Fields_TT")]
    GparamEditor_Add_Missing_Fields,

    [Display(Name = "KEY_GparamEditor_Execute_Quick_Edit", Description = "KEY_GparamEditor_Execute_Quick_Edit_TT")]
    GparamEditor_Execute_Quick_Edit,

    [Display(Name = "KEY_GparamEditor_Generate_Quick_Edit", Description = "KEY_GparamEditor_Generate_Quick_Edit_TT")]
    GparamEditor_Generate_Quick_Edit,

    [Display(Name = "KEY_GparamEditor_Clear_Quick_Edit", Description = "KEY_GparamEditor_Clear_Quick_Edit_TT")]
    GparamEditor_Clear_Quick_Edit,

    // Material Editor

    // Texture Viewer
    [Display(Name = "KEY_TextureViewer_Export_Texture", Description = "KEY_TextureViewer_Export_Texture_TT")]
    TextureViewer_Export_Texture,

    [Display(Name = "KEY_TextureViewer_Reset_Zoom_Level", Description = "KEY_TextureViewer_Reset_Zoom_Level_TT")]
    TextureViewer_Reset_Zoom_Level,

    // Developer
    [Display(Name = "KEY_CaptureThumbnailImage", Description = "KEY_CaptureThumbnailImage_TT")]
    CaptureThumbnailImage
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
        InputManager.Bind(KeybindID.Add, new() { Key = Key.Y, Shift = true });
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
        InputManager.Bind(KeybindID.ModelEditor_Select_Primitives_Only, new() { Key = Key.Z });

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

        // Developer
        InputManager.Bind(KeybindID.CaptureThumbnailImage, new() { Key = Key.F8 });
    }
}

public static class KeybindMetadata
{
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
        { KeybindID.Add, InputCategory.Contextual },
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
        { KeybindID.ModelEditor_Select_Primitives_Only, InputCategory.ModelEditor },

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

        // Developer
        { KeybindID.CaptureThumbnailImage, InputCategory.Developer },
    };
}

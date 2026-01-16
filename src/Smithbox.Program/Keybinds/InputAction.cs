using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Keybinds;


public enum InputAction
{
    // Viewport
    [Display(Name = "Move Forward")]
    MoveForward,

    [Display(Name = "Move Backward")]
    MoveBackward,

    [Display(Name = "Move Left")]
    MoveLeft,

    [Display(Name = "Move Right")]
    MoveRight,

    [Display(Name = "Move Up")]
    MoveUp,

    [Display(Name = "Move Down")]
    MoveDown,

    [Display(Name = "Frame")]
    Frame,

    [Display(Name = "Pull")]
    Pull,

    [Display(Name = "Reset")]
    Reset,

    [Display(Name = "Cycle Gizmo Translation Mode")]
    Cycle_Gizmo_Translation_Mode,

    [Display(Name = "Cycle Gizmo Rotation Mode")]
    Cycle_Gizmo_Rotation_Mode,

    [Display(Name = "Cycle Gizmo Origin Mode")]
    Cycle_Gizmo_Origin_Mode,

    [Display(Name = "Cycle Gizmo Space Mode")]
    Cycle_Gizmo_Space_Mode,

    [Display(Name = "Cycle Render Outline Mode")]
    Cycle_Render_Outline_Mode,

    // Common Actions
    [Display(Name = "Save")]
    Save,

    [Display(Name = "Undo")]
    Undo,

    [Display(Name = "Redo")]
    Redo,

    [Display(Name = "Undo (continuous)")]
    Undo_Repeat,

    [Display(Name = "Redo (continuous)")]
    Redo_Repeat,

    [Display(Name = "Up")]
    Up,

    [Display(Name = "Down")]
    Down,

    [Display(Name = "Left")]
    Left,

    [Display(Name = "Right")]
    Right,

    // Contextual Actions
    [Display(Name = "Select All")]
    SelectAll,

    [Display(Name = "Copy")]
    Copy,

    [Display(Name = "Paste")]
    Paste,

    [Display(Name = "Duplicate")]
    Duplicate,

    [Display(Name = "Delete")]
    Delete,

    [Display(Name = "Jump")]
    Jump,

    [Display(Name = "Move Up")]
    Reorder_Up,

    [Display(Name = "Move Down")]
    Reorder_Down,

    [Display(Name = "Move to Top")]
    Reorder_Top,

    [Display(Name = "Move to Bottom")]
    Reorder_Bottom,

    [Display(Name = "Apply to All")]
    Apply_to_All,

    // Map Editor
    [Display(Name = "Toggle World Map Menu")]
    MapEditor_Toggle_World_Map_Menu,

    [Display(Name = "Reset World Map Zoom Level")]
    MapEditor_Reset_World_Map_Zoom_Level,

    [Display(Name = "Create Map Object")]
    MapEditor_Create_Map_Object,

    [Display(Name = "Duplicate to Map")]
    MapEditor_Duplicate_To_Map,

    [Display(Name = "Rotate Selection (+ x-axis)")]
    MapEditor_Rotate_X_Axis,

    [Display(Name = "Rotate Selection (+ y-axis)")]
    MapEditor_Rotate_Y_Axis,

    [Display(Name = "Pivot Selection (+ y-axis)")]
    MapEditor_Rotate_Pivot_Y_Axis,

    [Display(Name = "Rotate Selection (- x-axis)")]
    MapEditor_Rotate_Minus_X_Axis,

    [Display(Name = "Rotate Selection (- y-axis)")]
    MapEditor_Rotate_Minus_Y_Axis,

    [Display(Name = "Pivot Selection (- y-axis)")]
    MapEditor_Rotate_Minus_Pivot_Y_Axis,

    [Display(Name = "Rotate Selection (fixed angle)")]
    MapEditor_Rotate_Fixed_Angle,

    [Display(Name = "Reset Selection Rotation")]
    MapEditor_Reset_Rotation,

    [Display(Name = "Scramble Selection")]
    MapEditor_Scramble,

    [Display(Name = "Replicate Selection")]
    MapEditor_Replicate,

    [Display(Name = "Cycle Render Type")]
    MapEditor_Cycle_Render_Type,

    [Display(Name = "Set Map Object to Dummy")]
    MapEditor_Make_Dummy_Object,

    [Display(Name = "Set Map Object to Normal")]
    MapEditor_Make_Normal_Object,

    [Display(Name = "Enable Map Object Game Presence")]
    MapEditor_Enable_Game_Presence,

    [Display(Name = "Disable Map Object Game Presence")]
    MapEditor_Disable_Game_Presence,

    [Display(Name = "Flip Selection Visibility")]
    MapEditor_Visibility_Flip,

    [Display(Name = "Enable Selection Visibility")]
    MapEditor_Visibility_Enable,

    [Display(Name = "Disable Selection Visibility")]
    MapEditor_Visibility_Disable,

    [Display(Name = "Flip Visibility for All")]
    MapEditor_Global_Visibility_Flip,

    [Display(Name = "Enable Visibility for All")]
    MapEditor_Global_Visibility_Enable,

    [Display(Name = "Disable Visibility for All")]
    MapEditor_Global_Visibility_Disable,

    [Display(Name = "Configure Grid Placement")]
    MapEditor_Configure_Grid_Placement,

    [Display(Name = "Cycle Selected Grid")]
    MapEditor_Cycle_Selected_Grid_Type,

    [Display(Name = "Move Selection to Primary Grid")]
    MapEditor_Move_to_Primary_Grid,

    [Display(Name = "Move Selection to Secondary Grid")]
    MapEditor_Move_to_Secondary_Grid,

    [Display(Name = "Move Selection to Tertiary Grid")]
    MapEditor_Move_to_Tertiary_Grid,

    [Display(Name = "Select All (Configurable)")]
    MapEditor_SelectAll_Configurable,

    [Display(Name = "Select All (Object Type)")]
    MapEditor_SelectAll_ObjectType,

    [Display(Name = "Select All (Model Name)")]
    MapEditor_SelectAll_ModelName,

    [Display(Name = "Create Selection Group")]
    MapEditor_Create_Selection_Group,

    [Display(Name = "Select Selection Group 0")]
    MapEditor_Select_Group_0,

    [Display(Name = "Select Selection Group 1")]
    MapEditor_Select_Group_1,

    [Display(Name = "Select Selection Group 2")]
    MapEditor_Select_Group_2,

    [Display(Name = "Select Selection Group 3")]
    MapEditor_Select_Group_3,

    [Display(Name = "Select Selection Group 4")]
    MapEditor_Select_Group_4,

    [Display(Name = "Select Selection Group 5")]
    MapEditor_Select_Group_5,

    [Display(Name = "Select Selection Group 6")]
    MapEditor_Select_Group_6,

    [Display(Name = "Select Selection Group 7")]
    MapEditor_Select_Group_7,

    [Display(Name = "Select Selection Group 8")]
    MapEditor_Select_Group_8,

    [Display(Name = "Select Selection Group 9")]
    MapEditor_Select_Group_9,

    [Display(Name = "Select Selection Group 10")]
    MapEditor_Select_Group_10,

    [Display(Name = "Cycle Rotation Increment Type")]
    MapEditor_Rotation_Increment_Cycle_Type,

    [Display(Name = "Cycle Rotation Increment Type Backwards")]
    MapEditor_Rotation_Increment_Cycle_Type_Backwards,

    [Display(Name = "Cycle Position Increment Type")]
    MapEditor_Position_Increment_Cycle_Type,

    [Display(Name = "Cycle Position Increment Type Backwards")]
    MapEditor_Position_Increment_Cycle_Type_Backwards,

    [Display(Name = "Toggle Position Increment Discrete Mode")]
    MapEditor_Position_Increment_Toggle_Discrete_Mode,

    [Display(Name = "Position Increment Move (+x)")]
    MapEditor_Position_Increment_Positive_X,

    [Display(Name = "Position Increment Move (-x)")]
    MapEditor_Position_Increment_Negative_X,

    [Display(Name = "Position Increment Move (+y)")]
    MapEditor_Position_Increment_Positive_Y,

    [Display(Name = "Position Increment Move (-y)")]
    MapEditor_Position_Increment_Negative_Y,

    [Display(Name = "Position Increment Move (+z)")]
    MapEditor_Position_Increment_Positive_Z,

    [Display(Name = "Position Increment Move (-z)")]
    MapEditor_Position_Increment_Negative_Z,

    [Display(Name = "Toggle Patrol Route Visuals")]
    MapEditor_Toggle_Patrol_Route_Visuals,

    [Display(Name = "View Display Group for Selection")]
    MapEditor_View_Display_Group,

    [Display(Name = "View Draw Group for Selection")]
    MapEditor_View_Draw_Group,

    [Display(Name = "Apply Display Group to Selection")]
    MapEditor_Apply_Display_Group,

    [Display(Name = "Apply Draw Group to Selection")]
    MapEditor_Apply_Draw_Group,

    [Display(Name = "Hide All Display Groups")]
    MapEditor_Hide_All_Display_Groups,

    [Display(Name = "Show All Display Groups")]
    MapEditor_Show_All_Display_Groups,

    [Display(Name = "Select Display Group Highlights")]
    MapEditor_Select_Display_Group_Highlights,

    // Model Editor

    // Param Editor
    [Display(Name = "Focus Searchbar")]
    ParamEditor_Focus_Searchbar,

    [Display(Name = "Apply Mass Edit")]
    ParamEditor_Apply_Mass_Edit,

    [Display(Name = "View Mass Edit")]
    ParamEditor_View_Mass_Edit,

    [Display(Name = "Import CSV")]
    ParamEditor_Import_CSV,

    [Display(Name = "Export CSV")]
    ParamEditor_Export_CSV,

    [Display(Name = "Export CSV (names)")]
    ParamEditor_Export_CSV_Names,

    [Display(Name = "Export CSV (param)")]
    ParamEditor_Export_CSV_Param,

    [Display(Name = "Export CSV (all rows)")]
    ParamEditor_Export_CSV_All_Rows,

    [Display(Name = "Export CSV (modified rows)")]
    ParamEditor_Export_CSV_Modified_Rows,

    [Display(Name = "Export CSV (selected rows)")]
    ParamEditor_Export_CSV_Selected_Rows,

    [Display(Name = "Reload All Params")]
    ParamEditor_Reload_All_Params,

    [Display(Name = "Reload Selected Param")]
    ParamEditor_Reload_Selected_Param,

    [Display(Name = "Sort Rows")]
    ParamEditor_RowList_Sort_Rows,

    [Display(Name = "Jump to Row ID")]
    ParamEditor_RowList_Jump_to_Row_ID,

    [Display(Name = "Inherit Referenced Row Name")]
    ParamEditor_RowList_Inherit_Referenced_Row_Name,

    // Text Editor
    [Display(Name = "Focus Searchbar")]
    TextEditor_Focus_Searchbar,

    [Display(Name = "Create New Entry")]
    TextEditor_Create_New_Entry,

    [Display(Name = "Duplicate (configurable)")]
    TextEditor_Configurable_Duplicate,

    // Gparam Editor
    [Display(Name = "Execute Quick Edit")]
    GparamEditor_Execute_Quick_Edit,

    [Display(Name = "Generate Quick Edit")]
    GparamEditor_Generate_Quick_Edit,

    [Display(Name = "Clear Quick Edit")]
    GparamEditor_Clear_Quick_Edit,

    // Material Editor

    // Texture Viewer
    [Display(Name = "Export Texture")]
    TextureViewer_Export_Texture,

    [Display(Name = "Reset Zoom Level")]
    TextureViewer_Reset_Zoom_Level
}
using System.ComponentModel.DataAnnotations;
using Veldrid;

namespace StudioCore.Keybinds;

public enum MousebindID
{
    // Viewport
    [Display(Name = "MOUSE_Viewport_Enable_Viewport_Movement", Description = "MOUSE_Viewport_Enable_Viewport_Movement_TT")]
    Viewport_Enable_Viewport_Movement,

    [Display(Name = "MOUSE_Viewport_Picking_Action", Description = "MOUSE_Viewport_Picking_Action_TT")]
    Viewport_Picking_Action,

    [Display(Name = "MOUSE_Viewport_Gizmo_Interaction_Action", Description = "MOUSE_Viewport_Gizmo_Interaction_Action_TT")]
    Viewport_Gizmo_Interaction_Action,

    // Map Editor
    [Display(Name = "MOUSE_MapEditor_Box_Drag_Start", Description = "MOUSE_MapEditor_Box_Drag_Start_TT")]
    MapEditor_Box_Drag_Start,

    [Display(Name = "MOUSE_MapEditor_Box_Drag_End", Description = "MOUSE_MapEditor_Box_Drag_End_TT")]
    MapEditor_Box_Drag_End,

    [Display(Name = "MOUSE_MapEditor_Display_Viewport_Context_Menu", Description = "MOUSE_MapEditor_Display_Viewport_Context_Menu_TT")]
    MapEditor_Display_Viewport_Context_Menu,

    [Display(Name = "MOUSE_MapEditor_Select_Map_In_World_Map", Description = "MOUSE_MapEditor_Select_Map_In_World_Map_TT")]
    MapEditor_Select_Map_In_World_Map,

    [Display(Name = "MOUSE_MapEditor_World_Map_Drag_Start", Description = "MOUSE_MapEditor_World_Map_Drag_Start_TT")]
    MapEditor_World_Map_Drag_Start,

    [Display(Name = "MOUSE_MapEditor_World_Map_Drag_End", Description = "MOUSE_MapEditor_World_Map_Drag_End_TT")]
    MapEditor_World_Map_Drag_End
}

public static class DefaultMouseBindings
{
    public static void CreateDefaultBindings()
    {
        // Viewport
        InputManager.BindMouseKey(
            MousebindID.Viewport_Enable_Viewport_Movement, new() { Key = MouseButton.Right });

        InputManager.BindMouseKey(
            MousebindID.Viewport_Picking_Action, new() { Key = MouseButton.Left });

        InputManager.BindMouseKey(
            MousebindID.Viewport_Gizmo_Interaction_Action, new() { Key = MouseButton.Left });

        // Map Editor
        InputManager.BindMouseKey(
            MousebindID.MapEditor_Box_Drag_Start, new() { Key = MouseButton.Left, Alt = true });

        InputManager.BindMouseKey(
            MousebindID.MapEditor_Box_Drag_End, new() { Key = MouseButton.Left });

        InputManager.BindMouseKey(
            MousebindID.MapEditor_Display_Viewport_Context_Menu, new() { Key = MouseButton.Button1 });

        InputManager.BindMouseKey(
            MousebindID.MapEditor_Select_Map_In_World_Map, new() { Key = MouseButton.Right });

        InputManager.BindMouseKey(
            MousebindID.MapEditor_World_Map_Drag_Start, new() { Key = MouseButton.Left });

        InputManager.BindMouseKey(
            MousebindID.MapEditor_World_Map_Drag_End, new() { Key = MouseButton.Left });
    }
}

public static class MousebindMetadata
{
    public static readonly Dictionary<MousebindID, InputCategory> Category = new()
    {
        { MousebindID.Viewport_Enable_Viewport_Movement, InputCategory.Viewport },
        { MousebindID.Viewport_Picking_Action, InputCategory.Viewport },
        { MousebindID.Viewport_Gizmo_Interaction_Action, InputCategory.Viewport },

        { MousebindID.MapEditor_Box_Drag_Start, InputCategory.MapEditor },
        { MousebindID.MapEditor_Box_Drag_End, InputCategory.MapEditor },
        { MousebindID.MapEditor_Display_Viewport_Context_Menu, InputCategory.MapEditor },
        { MousebindID.MapEditor_Select_Map_In_World_Map, InputCategory.MapEditor },
        { MousebindID.MapEditor_World_Map_Drag_Start, InputCategory.MapEditor },
        { MousebindID.MapEditor_World_Map_Drag_End, InputCategory.MapEditor },
    };
}

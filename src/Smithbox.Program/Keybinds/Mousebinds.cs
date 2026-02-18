using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Keybinds;

public enum MousebindID
{
    // Viewport
    Viewport_Enable_Viewport_Movement,
    Viewport_Picking_Action,
    Viewport_Gizmo_Interaction_Action,

    // Map Editor
    MapEditor_Box_Drag_Start,
    MapEditor_Box_Drag_End,
    MapEditor_Display_Viewport_Context_Menu,
    MapEditor_Select_Map_In_World_Map,
    MapEditor_World_Map_Drag_Start,
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
    public static readonly Dictionary<MousebindID, (string, string)> Presentation = new()
    {
        // Viewport
        { MousebindID.Viewport_Enable_Viewport_Movement, (
            "Enable Viewport Movement",
            "The button that is down to enable viewport movement input."
        ) },
        { MousebindID.Viewport_Picking_Action, (
            "Select Map Object",
            "The button that is pressed to select a map object in the viewport."
        ) },
        { MousebindID.Viewport_Gizmo_Interaction_Action, (
            "Interact with Gizmo",
            "The button that is down to interact with a gizmo control."
        ) },

        // Map Editor
        { MousebindID.MapEditor_Display_Viewport_Context_Menu, (
            "Display Viewport Context Menu",
            "The button that is pressed to open the viewport context menu."
        ) },
        { MousebindID.MapEditor_Box_Drag_Start, (
            "Start Box Drag",
            "The button that is down to signal the start of the box selection drag."
        ) },
        { MousebindID.MapEditor_Box_Drag_End, (
            "End Box Drag",
            "The button that is released to signal the end of the box selection drag."
        ) },
        { MousebindID.MapEditor_Select_Map_In_World_Map, (
            "Select Map in World Map",
            "The button that is pressed to select the maps to filter by when in the World Map menu."
        ) },
        { MousebindID.MapEditor_World_Map_Drag_Start, (
            "Start World Map Drag",
            "The button that is down to signal the start of the world map drag."
        ) },
        { MousebindID.MapEditor_World_Map_Drag_End, (
            "End World Map Drag",
            "The button that is released to signal the end of the world map drag."
        ) },
    };


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

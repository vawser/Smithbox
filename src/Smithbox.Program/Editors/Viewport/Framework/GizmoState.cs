using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;

namespace StudioCore.Editors.Viewport;

/// <summary>
/// Gizmo configuration state is shared between all viewports.
/// </summary>
public static class GizmoState
{
    public static GizmosMode Mode = GizmosMode.Translate;
    public static GizmosSpace Space = GizmosSpace.Local;
    public static GizmosOrigin Origin = GizmosOrigin.World;

    public static void OnMenu(VulkanViewport viewport)
    {
        if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Gizmo_Menu_Action_Display")}##displayGizmos"))
        {
            CFG.Current.Viewport_Render_Gizmos = !CFG.Current.Viewport_Render_Gizmos;
            viewport.DelayPicking();
        }
        UIHelper.ShowActiveStatus(CFG.Current.Viewport_Render_Gizmos);
        UIHelper.Tooltip(LOC.Get("VIEWPORT_Gizmo_Menu_Action_Display_TT"));

        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Gizmo_Menu_Input_Size")}##gizmoScale", ref CFG.Current.Viewport_Gizmo_Size_Distance_Scale, 0.01f, 5.0f, ImGuiSliderFlags.AlwaysClamp);

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            if (CFG.Current.Viewport_Gizmo_Size_Distance_Scale < 0.01)
            {
                CFG.Current.Viewport_Gizmo_Size_Distance_Scale = 0.01f;
            }
            if (CFG.Current.Viewport_Gizmo_Size_Distance_Scale > 5.0)
            {
                CFG.Current.Viewport_Gizmo_Size_Distance_Scale = 5.0f;
            }
        }

        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Gizmo_Menu_Header_Mode")}##modeMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Gizmo_Menu_Action_Translate")}##translateAction", InputManager.GetHint(KeybindID.Cycle_Gizmo_Translation_Mode)))
            {
                Mode = GizmosMode.Translate;
                viewport.DelayPicking();
            }
            UIHelper.Tooltip(LOC.Get("VIEWPORT_Gizmo_Menu_Action_Translate_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Gizmo_Menu_Action_Rotate")}##rotateAction", InputManager.GetHint(KeybindID.Cycle_Gizmo_Rotation_Mode)))
            {
                Mode = GizmosMode.Rotate;
                viewport.DelayPicking();
            }
            UIHelper.Tooltip(LOC.Get("VIEWPORT_Gizmo_Menu_Action_Rotate_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Gizmo_Menu_Action_Scale")}##scaleAction", InputManager.GetHint(KeybindID.Cycle_Gizmo_Scale_Mode)))
            {
                Mode = GizmosMode.Scale;
                viewport.DelayPicking();
            }
            UIHelper.Tooltip(LOC.Get("VIEWPORT_Gizmo_Menu_Action_Scale_TT"));

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Gizmo_Menu_Header_Space")}##spaceMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Gizmo_Menu_Action_Space_Local")}##spaceLocalAction", InputManager.GetHint(KeybindID.Cycle_Gizmo_Space_Mode)))
            {
                Space = GizmosSpace.Local;
                viewport.DelayPicking();
            }
            UIHelper.Tooltip(LOC.Get("VIEWPORT_Gizmo_Menu_Action_Space_Local_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Gizmo_Menu_Action_Space_World")}##spaceWorldAction", InputManager.GetHint(KeybindID.Cycle_Gizmo_Space_Mode)))
            {
                Space = GizmosSpace.World;
                viewport.DelayPicking();
            }
            UIHelper.Tooltip(LOC.Get("VIEWPORT_Gizmo_Menu_Action_Space_World_TT"));

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Gizmo_Menu_Header_Origin")}##originMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Gizmo_Menu_Action_Origin_World")}##originWorldAction", InputManager.GetHint(KeybindID.Cycle_Gizmo_Origin_Mode)))
            {
                Origin = GizmosOrigin.World;
                viewport.DelayPicking();
            }
            UIHelper.Tooltip(LOC.Get("VIEWPORT_Gizmo_Menu_Action_Origin_World_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Gizmo_Menu_Action_Origin_Bounding_Box")}##originBoundingBoxAction", InputManager.GetHint(KeybindID.Cycle_Gizmo_Origin_Mode)))
            {
                Origin = GizmosOrigin.BoundingBox;
                viewport.DelayPicking();
            }
            UIHelper.Tooltip(LOC.Get("VIEWPORT_Gizmo_Menu_Action_Origin_Bounding_Box_TT"));

            ImGui.EndMenu();
        }
    }

    public static void OnShortcut()
    {
        if (FocusManager.IsFocus(EditorFocusContext.MapEditor_Viewport) || FocusManager.IsFocus(EditorFocusContext.ModelEditor_Viewport))
        {
            if (InputManager.IsPressed(KeybindID.Cycle_Gizmo_Translation_Mode))
            {
                Mode = GizmosMode.Translate;
            }
            if (InputManager.IsPressed(KeybindID.Cycle_Gizmo_Rotation_Mode))
            {
                Mode = GizmosMode.Rotate;
            }
            if (InputManager.IsPressed(KeybindID.Cycle_Gizmo_Scale_Mode))
            {
                Mode = GizmosMode.Scale;
            }

            // Cycle Gizmo Origin Mode
            if (InputManager.IsPressed(KeybindID.Cycle_Gizmo_Origin_Mode))
            {
                if (Origin == GizmosOrigin.World)
                {
                    Origin = GizmosOrigin.BoundingBox;
                }
                else if (Origin == GizmosOrigin.BoundingBox)
                {
                    Origin = GizmosOrigin.World;
                }
            }

            // Cycle Gizmo Space Mode
            if (InputManager.IsPressed(KeybindID.Cycle_Gizmo_Space_Mode))
            {
                if (Space == GizmosSpace.Local)
                {
                    Space = GizmosSpace.World;
                }
                else if (Space == GizmosSpace.World)
                {
                    Space = GizmosSpace.Local;
                }
            }
        }
    }

    public enum Axis
    {
        None,
        PosX,
        PosY,
        PosZ,
        PosXY,
        PosYZ,
        PosXZ
    }

    public enum GizmosMode
    {
        Translate,
        Rotate,
        Scale
    }

    /// <summary>
    ///     The origin where the gizmos is located relative to the object
    /// </summary>
    public enum GizmosOrigin
    {
        /// <summary>
        ///     The gizmos originates at the selected's world space position
        /// </summary>
        World,

        /// <summary>
        ///     The gizmos originates at the selected's bounding box center
        /// </summary>
        BoundingBox,

        /// <summary>
        ///     The gizmos originates at the selected's parent if one exists
        /// </summary>
        Parent
    }

    /// <summary>
    ///     The rotation space that the gizmos works in
    /// </summary>
    public enum GizmosSpace
    {
        /// <summary>
        ///     The gizmos is rotated by the local rotation
        /// </summary>
        Local,

        /// <summary>
        ///     The gizmos rotation always originates at identity
        /// </summary>
        World
    }

}

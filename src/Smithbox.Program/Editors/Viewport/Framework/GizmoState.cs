using Hexa.NET.ImGui;
using StudioCore.Application;
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

    public static void OnMenu()
    {
        if (ImGui.MenuItem("Display"))
        {
            CFG.Current.Viewport_Render_Gizmos = !CFG.Current.Viewport_Render_Gizmos;
        }
        UIHelper.ShowActiveStatus(CFG.Current.Viewport_Render_Gizmos);
        UIHelper.Tooltip("Toggle the display of gizmos.");

        ImGui.SliderFloat("Size##gizmoScale", ref CFG.Current.Viewport_Gizmo_Size_Distance_Scale, 0.01f, 5.0f, ImGuiSliderFlags.AlwaysClamp);

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

        if (ImGui.BeginMenu("Mode"))
        {
            if (ImGui.MenuItem("Translate", InputManager.GetHint(KeybindID.Cycle_Gizmo_Translation_Mode)))
            {
                Mode = GizmosMode.Translate;
            }
            UIHelper.Tooltip($"Set the gizmo to Translation mode.");

            if (ImGui.MenuItem("Rotate", InputManager.GetHint(KeybindID.Cycle_Gizmo_Rotation_Mode)))
            {
                Mode = GizmosMode.Rotate;
            }
            UIHelper.Tooltip($"Set the gizmo to Rotation mode.");

            if (ImGui.MenuItem("Scale", InputManager.GetHint(KeybindID.Cycle_Gizmo_Scale_Mode)))
            {
                Mode = GizmosMode.Scale;
            }
            UIHelper.Tooltip($"Set the gizmo to Scale mode.");

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Space"))
        {
            if (ImGui.MenuItem("Local", InputManager.GetHint(KeybindID.Cycle_Gizmo_Space_Mode)))
            {
                Space = GizmosSpace.Local;
            }
            UIHelper.Tooltip($"Place the gizmo origin based on the selection's local position.");

            if (ImGui.MenuItem("World", InputManager.GetHint(KeybindID.Cycle_Gizmo_Space_Mode)))
            {
                Space = GizmosSpace.World;
            }
            UIHelper.Tooltip($"Place the gizmo origin based on the selection's world position.");

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Origin"))
        {
            if (ImGui.MenuItem("World", InputManager.GetHint(KeybindID.Cycle_Gizmo_Origin_Mode)))
            {
                Origin = GizmosOrigin.World;
            }
            UIHelper.Tooltip($"Orient the gizmo origin based on the world position.");

            if (ImGui.MenuItem("Bounding Box", InputManager.GetHint(KeybindID.Cycle_Gizmo_Origin_Mode)))
            {
                Origin = GizmosOrigin.BoundingBox;
            }
            UIHelper.Tooltip($"Orient the gizmo origin based on the bounding box.");

            ImGui.EndMenu();
        }
    }

    public static void OnShortcut()
    {
        // Cycle Gizmo Translation Mode
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

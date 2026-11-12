using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;

namespace StudioCore.Editors.Viewport;

/// <summary>
///     The Camera section of the Rendering tool category: the view mode, the preset views, the projection's
///     parameters, movement, the slice and the camera clip of the editor's current viewport. Most values are kept in
///     the settings and shared by every viewport; the speeds, pan, size, oblique values and slice are the camera's own,
///     with the settings as the defaults for new cameras.
/// </summary>
public static class ViewportCameraTool
{
    public static void Display(VulkanViewport viewport)
    {
        var camera = viewport.ViewportCamera;

        GUI.WrappedText(LOC.Get("VIEWPORT_Camera_Tool_Hint"));

        // View Mode
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("VIEWPORT_Menubar_View_Mode"),
            LOC.Get("VIEWPORT_Menubar_View_Mode_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##viewModeCombo", LOC.Get(camera.ViewMode.GetDisplayName())))
        {
            foreach (var type in Enum.GetValues<ViewMode>())
            {
                if (ImGui.Selectable($"{LOC.Get(type.GetDisplayName())}##viewMode{type}", camera.ViewMode == type))
                {
                    camera.SetProjectionType(type);
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(WithShortcut(LOC.Get("VIEWPORT_Menubar_View_Mode_TT"), KeybindID.Viewport_Cycle_View_Mode));

        // Preset Views
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("VIEWPORT_Menubar_Preset_Views"),
            LOC.Get("VIEWPORT_Menubar_Preset_Views_TT"));

        GUI.MultiButtonInput("cameraPresetViews",
            "presetViewTop", LOC.Get(ViewportPresetView.Top.GetDisplayName()),
            WithShortcut(LOC.Get("KEY_Viewport_View_Top_TT"), KeybindID.Viewport_View_Top),
            () => camera.SetPresetView(ViewportPresetView.Top),

            "presetViewBottom", LOC.Get(ViewportPresetView.Bottom.GetDisplayName()),
            WithShortcut(LOC.Get("KEY_Viewport_View_Bottom_TT"), KeybindID.Viewport_View_Bottom),
            () => camera.SetPresetView(ViewportPresetView.Bottom),

            "presetViewFront", LOC.Get(ViewportPresetView.Front.GetDisplayName()),
            WithShortcut(LOC.Get("KEY_Viewport_View_Front_TT"), KeybindID.Viewport_View_Front),
            () => camera.SetPresetView(ViewportPresetView.Front),

            "presetViewBack", LOC.Get(ViewportPresetView.Back.GetDisplayName()),
            WithShortcut(LOC.Get("KEY_Viewport_View_Back_TT"), KeybindID.Viewport_View_Back),
            () => camera.SetPresetView(ViewportPresetView.Back),

            "presetViewRight", LOC.Get(ViewportPresetView.Right.GetDisplayName()),
            WithShortcut(LOC.Get("KEY_Viewport_View_Right_TT"), KeybindID.Viewport_View_Right),
            () => camera.SetPresetView(ViewportPresetView.Right),

            "presetViewLeft", LOC.Get(ViewportPresetView.Left.GetDisplayName()),
            WithShortcut(LOC.Get("KEY_Viewport_View_Left_TT"), KeybindID.Viewport_View_Left),
            () => camera.SetPresetView(ViewportPresetView.Left),

            "snapViewToAxis", LOC.Get("VIEWPORT_Menubar_Snap_View_To_Axis"),
            WithShortcut(LOC.Get("VIEWPORT_Menubar_Snap_View_To_Axis_TT"), KeybindID.Viewport_Snap_View_To_Axis),
            camera.SnapToNearestAxis);

        SettingCheckbox($"{LOC.Get("VIEWPORT_Menubar_Preset_View_Switch_To_Orthographic")}##presetSwitchOrtho",
            LOC.Get("VIEWPORT_Menubar_Preset_View_Switch_To_Orthographic_TT"),
            () => CFG.Current.Viewport_Preset_View_Switch_To_Orthographic,
            v => CFG.Current.Viewport_Preset_View_Switch_To_Orthographic = v);

        // View Parameters
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("VIEWPORT_Menubar_View_Parameters"),
            LOC.Get("VIEWPORT_Menubar_View_Parameters_TT"));

        // AlwaysClamp keeps typed (ctrl+click) values inside each slider's range, so no clamping is needed after
        if (camera.ViewMode is ViewMode.Perspective)
        {
            PerspectiveParameters(camera);
        }
        else
        {
            OrthographicParameters(camera);
        }

        // Oblique
        if (camera.ViewMode is ViewMode.Oblique)
        {
            ObliqueParameters(camera);
        }

        // Slice
        if (camera.ViewMode is not ViewMode.Perspective)
        {
            SliceParameters(camera);
        }

        // Camera Clip
        CameraClipParameters(camera);

        // Reset
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("VIEWPORT_Camera_Tool_Reset_Header"),
            LOC.Get("VIEWPORT_Camera_Tool_Reset_Header_TT"));

        GUI.MultiButtonInput("cameraReset",
            "resetCameraSettings", LOC.Get("VIEWPORT_Menubar_Action_Reset_Camera_Settings"),
            LOC.Get("VIEWPORT_Menubar_Action_Reset_Camera_Settings_TT"),
            () => ResetCameraSettings(camera));
    }

    // The settings are properties, which ImGui can't take by ref, so these read and write them around the input

    private const ImGuiSliderFlags LogClamp = ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic;

    private static void SettingCheckbox(string label, string tooltip, Func<bool> get, Action<bool> set)
    {
        var value = get();
        if (ImGui.Checkbox(label, ref value))
        {
            set(value);
        }
        GUI.Tooltip(tooltip);
    }

    private static void SettingSlider(string label, string tooltip, Func<float> get, Action<float> set,
        float min, float max, string format, ImGuiSliderFlags flags)
    {
        var value = get();
        if (ImGui.SliderFloat(label, ref value, min, max, format, flags))
        {
            set(value);
        }
        GUI.Tooltip(tooltip);
    }

    private static void SettingDrag(string label, string tooltip, Func<float> get, Action<float> set,
        float speed, float min, float max, string format)
    {
        var value = get();
        if (ImGui.DragFloat(label, ref value, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp))
        {
            set(value);
        }
        GUI.Tooltip(tooltip);
    }

    /// <summary>A tooltip with the shortcut of a keybind on the end, if it has one</summary>
    private static string WithShortcut(string tooltip, KeybindID key)
    {
        var hint = InputManager.GetHint(key);

        return string.IsNullOrEmpty(hint) ? tooltip : LOC.Get("VIEWPORT_Camera_Tool_Shortcut", tooltip, hint);
    }

    private static void PerspectiveParameters(ViewportCamera camera)
    {
        SettingSlider($"{LOC.Get("VIEWPORT_Menubar_View_Near_Clip")}##nearClip", LOC.Get("VIEWPORT_Menubar_View_Near_Clip_TT"),
            () => CFG.Current.Viewport_Perspective_Near_Clip, v => CFG.Current.Viewport_Perspective_Near_Clip = v,
            0.01f, 100.0f, "%.2f", LogClamp);

        SettingSlider($"{LOC.Get("VIEWPORT_Menubar_View_Far_Clip")}##farClip", LOC.Get("VIEWPORT_Menubar_View_Far_Clip_TT"),
            () => CFG.Current.Viewport_Perspective_Far_Clip, v => CFG.Current.Viewport_Perspective_Far_Clip = v,
            0.01f, 1000000.0f, "%.0f", LogClamp);

        SettingSlider($"{LOC.Get("VIEWPORT_Menubar_Camera_FOV")}##cameraFov", LOC.Get("VIEWPORT_Menubar_Camera_FOV_TT"),
            () => CFG.Current.Viewport_Camera_FOV, v => CFG.Current.Viewport_Camera_FOV = v,
            40.0f, 140.0f, "%.0f", ImGuiSliderFlags.AlwaysClamp);

        // Camera Movement
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("VIEWPORT_Menubar_Camera_Movement"),
            LOC.Get("VIEWPORT_Menubar_Camera_Movement_TT"));

        SettingSlider($"{LOC.Get("VIEWPORT_Menubar_Camera_Sensitivity")}##cameraSensitivity",
            LOC.Get("VIEWPORT_Menubar_Camera_Sensitivity_TT"),
            () => CFG.Current.Viewport_Camera_Sensitivity, v => CFG.Current.Viewport_Camera_Sensitivity = v,
            0.0f, 0.1f, "%.3f", ImGuiSliderFlags.AlwaysClamp);

        // The speeds span four orders of magnitude, so the sliders are logarithmic to keep the low end usable
        if (ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Camera_Speed_Slow")}##cameraSpeedSlow",
            ref camera.CameraMoveSpeed_Slow, 0.1f, 9999.0f, "%.1f", ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic))
        {
            CFG.Current.Viewport_Camera_MoveSpeed_Slow = camera.CameraMoveSpeed_Slow;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_Speed_Slow_TT"));

        if (ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Camera_Speed_Normal")}##cameraSpeedNormal",
            ref camera.CameraMoveSpeed_Normal, 0.1f, 9999.0f, "%.1f", ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic))
        {
            CFG.Current.Viewport_Camera_MoveSpeed_Normal = camera.CameraMoveSpeed_Normal;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_Speed_Normal_TT"));

        if (ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Camera_Speed_Fast")}##cameraSpeedFast",
            ref camera.CameraMoveSpeed_Fast, 0.1f, 9999.0f, "%.1f", ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic))
        {
            CFG.Current.Viewport_Camera_MoveSpeed_Fast = camera.CameraMoveSpeed_Fast;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_Speed_Fast_TT"));
    }

    private static void OrthographicParameters(ViewportCamera camera)
    {
        // A slider across two million units can't be moved finely, so the clips are dragged instead
        SettingDrag($"{LOC.Get("VIEWPORT_Menubar_Ortho_Near_Clip")}##orthoNearClip", LOC.Get("VIEWPORT_Menubar_Ortho_Near_Clip_TT"),
            () => CFG.Current.Viewport_Orthographic_Near_Clip, v => CFG.Current.Viewport_Orthographic_Near_Clip = v,
            10.0f, -1000000.0f, 1000000.0f, "%.0f");

        SettingDrag($"{LOC.Get("VIEWPORT_Menubar_Ortho_Far_Clip")}##orthoFarClip", LOC.Get("VIEWPORT_Menubar_Ortho_Far_Clip_TT"),
            () => CFG.Current.Viewport_Orthographic_Far_Clip, v => CFG.Current.Viewport_Orthographic_Far_Clip = v,
            10.0f, -1000000.0f, 1000000.0f, "%.0f");

        if (ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Ortho_Size")}##orthoSize",
            ref camera.OrthographicSize, camera.OrthographicMinSize, camera.OrthographicMaxSize, "%.1f",
            ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic))
        {
            CFG.Current.Viewport_DefaultOrthographicSize = camera.OrthographicSize;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Ortho_Size_TT"));

        if (ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Ortho_Pan_Sensitivity")}##orthoPanSensitivity",
            ref camera.PanSensitivity, 1.0f, 100.0f, "%.1f", ImGuiSliderFlags.AlwaysClamp))
        {
            CFG.Current.Viewport_MousePan_Sensitivity = camera.PanSensitivity;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Ortho_Pan_Sensitivity_TT"));

        SettingCheckbox($"{LOC.Get("VIEWPORT_Menubar_Ortho_Zoom_To_Cursor")}##orthoZoomToCursor",
            LOC.Get("VIEWPORT_Menubar_Ortho_Zoom_To_Cursor_TT"),
            () => CFG.Current.Viewport_Orthographic_Zoom_To_Cursor, v => CFG.Current.Viewport_Orthographic_Zoom_To_Cursor = v);

        SettingCheckbox($"{LOC.Get("VIEWPORT_Menubar_Ortho_Scale_Bar")}##orthoScaleBar",
            LOC.Get("VIEWPORT_Menubar_Ortho_Scale_Bar_TT"),
            () => CFG.Current.Viewport_DisplayScaleBar, v => CFG.Current.Viewport_DisplayScaleBar = v);
    }

    private static void ObliqueParameters(ViewportCamera camera)
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("VIEWPORT_ENUM_ViewMode_Oblique"),
            LOC.Get("VIEWPORT_Camera_Tool_Oblique_Header_TT"));

        if (ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Oblique_Angle")}##obliqueAngle",
            ref camera.ObliqueAngle, 0.0f, 90.0f, "%.1f", ImGuiSliderFlags.AlwaysClamp))
        {
            CFG.Current.Viewport_DefaultObliqueAngle = camera.ObliqueAngle;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Oblique_Angle_TT"));

        if (ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Oblique_Scaling")}##obliqueScaling",
            ref camera.ObliqueScaling, 0.0f, 1.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp))
        {
            CFG.Current.Viewport_DefaultObliqueScaling = camera.ObliqueScaling;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Oblique_Scaling_TT"));

        SettingCheckbox($"{LOC.Get("VIEWPORT_Menubar_Oblique_Snap_To_Axis")}##obliqueSnapToAxis",
            LOC.Get("VIEWPORT_Menubar_Oblique_Snap_To_Axis_TT"),
            () => CFG.Current.Viewport_Oblique_Snap_To_Axis, v => CFG.Current.Viewport_Oblique_Snap_To_Axis = v);

        GUI.MultiButtonInput("cameraObliquePresets",
            "obliqueCabinet", LOC.Get("VIEWPORT_Menubar_Oblique_Cabinet"), LOC.Get("VIEWPORT_Menubar_Oblique_Cabinet_TT"),
            () => camera.ApplyObliquePreset(ViewportCamera.ObliquePreset.Cabinet),

            "obliqueCavalier", LOC.Get("VIEWPORT_Menubar_Oblique_Cavalier"), LOC.Get("VIEWPORT_Menubar_Oblique_Cavalier_TT"),
            () => camera.ApplyObliquePreset(ViewportCamera.ObliquePreset.Cavalier),

            "obliquePlan", LOC.Get("VIEWPORT_Menubar_Oblique_Plan"), LOC.Get("VIEWPORT_Menubar_Oblique_Plan_TT"),
            () => camera.ApplyObliquePreset(ViewportCamera.ObliquePreset.Plan));
    }

    private static void SliceParameters(ViewportCamera camera)
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("VIEWPORT_Menubar_Slice"),
            LOC.Get("VIEWPORT_Menubar_Slice_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_Slice_Enabled")}##sliceEnabled", ref camera.SliceEnabled);
        GUI.Tooltip(WithShortcut(LOC.Get("VIEWPORT_Menubar_Slice_Enabled_TT"), KeybindID.Viewport_Toggle_Slice));

        ImGui.BeginDisabled(!camera.SliceEnabled);

        var sliceThickness = camera.SliceHalfDepth * 2.0f;
        if (ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Slice_Thickness")}##sliceThickness",
            ref sliceThickness, ViewportCamera.SliceMinHalfDepth * 2.0f, ViewportCamera.SliceMaxHalfDepth * 2.0f,
            "%.2f", ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic))
        {
            camera.SetSliceHalfDepth(sliceThickness * 0.5f);
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Slice_Thickness_TT"));

        var sliceDistance = camera.FocusDistance;
        if (ImGui.DragFloat($"{LOC.Get("VIEWPORT_Menubar_Slice_Distance")}##sliceDistance",
            ref sliceDistance, MathF.Max(camera.SliceHalfDepth * 0.05f, 0.01f), -100000.0f, 100000.0f, "%.2f"))
        {
            camera.MoveFocus(sliceDistance - camera.FocusDistance);
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Slice_Distance_TT"));

        ImGui.EndDisabled();
    }

    /// <summary>
    ///     Drops geometry within a radius of the camera. Perspective only: an orthographic camera sits in the middle of
    ///     what it draws, so the clip would cut a hole there (SceneRenderPipeline skips it).
    /// </summary>
    private static void CameraClipParameters(ViewportCamera camera)
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("VIEWPORT_Menubar_Camera_Clip"),
            LOC.Get("VIEWPORT_Menubar_Camera_Clip_TT"));

        if (camera.ViewMode is not ViewMode.Perspective)
        {
            GUI.WrappedText(LOC.Get("VIEWPORT_Menubar_Camera_Clip_Perspective_Only"));
            return;
        }

        SettingCheckbox($"{LOC.Get("VIEWPORT_Menubar_Camera_Clip_Enabled")}##cameraClipEnabled",
            LOC.Get("VIEWPORT_Menubar_Camera_Clip_Enabled_TT"),
            () => CFG.Current.Viewport_Camera_Clip_Enabled, v => CFG.Current.Viewport_Camera_Clip_Enabled = v);

        SettingSlider($"{LOC.Get("VIEWPORT_Menubar_Camera_Clip_Radius")}##cameraClipRadius",
            LOC.Get("VIEWPORT_Menubar_Camera_Clip_Radius_TT"),
            () => CFG.Current.Viewport_Camera_Clip_Radius, v => CFG.Current.Viewport_Camera_Clip_Radius = v,
            0.1f, 1000.0f, "%.1f", LogClamp);
    }

    /// <summary>
    ///     Puts the camera settings back to their defaults. The speed, pan, size and oblique sliders edit the camera's
    ///     own fields, which are only read from the settings when the camera is made, so those are reset too.
    /// </summary>
    private static void ResetCameraSettings(ViewportCamera camera)
    {
        CFG.Current.Viewport_Camera_FOV = CFG.Default.Viewport_Camera_FOV;
        CFG.Current.Viewport_Camera_Sensitivity = CFG.Default.Viewport_Camera_Sensitivity;

        CFG.Current.Viewport_Perspective_Near_Clip = CFG.Default.Viewport_Perspective_Near_Clip;
        CFG.Current.Viewport_Perspective_Far_Clip = CFG.Default.Viewport_Perspective_Far_Clip;

        CFG.Current.Viewport_Camera_Clip_Enabled = CFG.Default.Viewport_Camera_Clip_Enabled;
        CFG.Current.Viewport_Camera_Clip_Radius = CFG.Default.Viewport_Camera_Clip_Radius;

        CFG.Current.Viewport_Camera_MoveSpeed_Slow = CFG.Default.Viewport_Camera_MoveSpeed_Slow;
        CFG.Current.Viewport_Camera_MoveSpeed_Normal = CFG.Default.Viewport_Camera_MoveSpeed_Normal;
        CFG.Current.Viewport_Camera_MoveSpeed_Fast = CFG.Default.Viewport_Camera_MoveSpeed_Fast;

        CFG.Current.Viewport_Orthographic_Near_Clip = CFG.Default.Viewport_Orthographic_Near_Clip;
        CFG.Current.Viewport_Orthographic_Far_Clip = CFG.Default.Viewport_Orthographic_Far_Clip;

        CFG.Current.Viewport_MousePan_Sensitivity = CFG.Default.Viewport_MousePan_Sensitivity;
        CFG.Current.Viewport_Orthographic_Zoom_To_Cursor = CFG.Default.Viewport_Orthographic_Zoom_To_Cursor;
        CFG.Current.Viewport_Preset_View_Switch_To_Orthographic = CFG.Default.Viewport_Preset_View_Switch_To_Orthographic;
        CFG.Current.Viewport_Oblique_Snap_To_Axis = CFG.Default.Viewport_Oblique_Snap_To_Axis;
        CFG.Current.Viewport_Slice_Half_Depth = CFG.Default.Viewport_Slice_Half_Depth;
        CFG.Current.Viewport_DisplayScaleBar = CFG.Default.Viewport_DisplayScaleBar;
        CFG.Current.Viewport_DefaultOrthographicSize = CFG.Default.Viewport_DefaultOrthographicSize;

        CFG.Current.Viewport_DefaultObliqueAngle = CFG.Default.Viewport_DefaultObliqueAngle;
        CFG.Current.Viewport_DefaultObliqueScaling = CFG.Default.Viewport_DefaultObliqueScaling;

        camera.CameraMoveSpeed_Slow = CFG.Current.Viewport_Camera_MoveSpeed_Slow;
        camera.CameraMoveSpeed_Normal = CFG.Current.Viewport_Camera_MoveSpeed_Normal;
        camera.CameraMoveSpeed_Fast = CFG.Current.Viewport_Camera_MoveSpeed_Fast;
        camera.PanSensitivity = CFG.Current.Viewport_MousePan_Sensitivity;
        camera.OrthographicSize = CFG.Current.Viewport_DefaultOrthographicSize;
        camera.ObliqueAngle = CFG.Current.Viewport_DefaultObliqueAngle;
        camera.ObliqueScaling = CFG.Current.Viewport_DefaultObliqueScaling;
        camera.SliceHalfDepth = CFG.Current.Viewport_Slice_Half_Depth;
    }
}

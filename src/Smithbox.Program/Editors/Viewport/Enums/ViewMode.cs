using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.Viewport;

public enum ViewMode
{
    [Display(Name = "VIEWPORT_ENUM_ViewMode_Perspective")]
    Perspective,
    [Display(Name = "VIEWPORT_ENUM_ViewMode_Orthographic")]
    Orthographic,
    [Display(Name = "VIEWPORT_ENUM_ViewMode_Oblique")]
    Oblique
}

/// <summary>
///     The axis-aligned views the camera can be put in, named by the direction they look along.
/// </summary>
public enum ViewportPresetView
{
    [Display(Name = "VIEWPORT_ENUM_PresetView_Top")]
    Top,
    [Display(Name = "VIEWPORT_ENUM_PresetView_Bottom")]
    Bottom,
    [Display(Name = "VIEWPORT_ENUM_PresetView_Front")]
    Front,
    [Display(Name = "VIEWPORT_ENUM_PresetView_Back")]
    Back,
    [Display(Name = "VIEWPORT_ENUM_PresetView_Right")]
    Right,
    [Display(Name = "VIEWPORT_ENUM_PresetView_Left")]
    Left
}
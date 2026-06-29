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
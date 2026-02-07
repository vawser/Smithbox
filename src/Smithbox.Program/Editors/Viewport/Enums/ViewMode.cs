using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.Viewport;

public enum ViewMode
{
    [Display(Name = "Perspective")]
    Perspective,
    [Display(Name = "Orthographic")]
    Orthographic,
    [Display(Name = "Oblique")]
    Oblique
}
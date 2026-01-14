using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MapEditor;

public enum ViewportTargetGridType
{
    [Display(Name = "Primary")]
    Primary = 0,
    [Display(Name = "Secondary")]
    Secondary = 1,
    [Display(Name = "Tertiary")]
    Tertiary = 2
}

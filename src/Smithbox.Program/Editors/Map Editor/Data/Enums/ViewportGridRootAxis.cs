using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MapEditor;

public enum ViewportGridRootAxis
{
    [Display(Name = "X")]
    X = 0,
    [Display(Name = "Y")]
    Y = 1,
    [Display(Name = "Z")]
    Z = 2
}

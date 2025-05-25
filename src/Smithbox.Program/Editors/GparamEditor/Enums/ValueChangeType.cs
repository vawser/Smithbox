using System.ComponentModel.DataAnnotations;

namespace StudioCore.GraphicsParamEditorNS;

public enum ValueChangeType
{
    [Display(Name = "Set")] Set,
    [Display(Name = "Addition")] Addition,
    [Display(Name = "Subtraction")] Subtraction,
    [Display(Name = "Multiplication")] Multiplication
}
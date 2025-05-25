using System.ComponentModel.DataAnnotations;

namespace StudioCore.GraphicsParamEditorNS;

public enum EditEffectType
{
    [Display(Name = "Set")] Set,
    [Display(Name = "Add")] Add,
    [Display(Name = "Subtract")] Subtract,
    [Display(Name = "Multiply")] Multiply,
    [Display(Name = "Set by Row")] SetByRow,
    [Display(Name = "Restore")] Restore,
    [Display(Name = "Random")] Random
}

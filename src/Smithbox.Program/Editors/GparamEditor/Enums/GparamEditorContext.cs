using System.ComponentModel.DataAnnotations;

namespace StudioCore.GraphicsParamEditorNS;

public enum GparamEditorContext
{
    [Display(Name = "None")] None,
    [Display(Name = "File")] File,
    [Display(Name = "Group")] Group,
    [Display(Name = "Field")] Field,
    [Display(Name = "Field Value")] FieldValue,
    [Display(Name = "Tool Window")] ToolWindow
}

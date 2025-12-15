using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.GparamEditor;

public enum GparamEditorContext
{
    [Display(Name = "None")] None,
    [Display(Name = "File")] File,
    [Display(Name = "Group")] Group,
    [Display(Name = "Field")] Field,
    [Display(Name = "Field Value")] FieldValue,
    [Display(Name = "Tool Window")] ToolWindow
}

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
public enum ValueChangeType
{
    [Display(Name = "Set")] Set,
    [Display(Name = "Addition")] Addition,
    [Display(Name = "Subtraction")] Subtraction,
    [Display(Name = "Multiplication")] Multiplication
}
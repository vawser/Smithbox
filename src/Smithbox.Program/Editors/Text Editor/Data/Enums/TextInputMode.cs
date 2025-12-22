using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

public enum TextInputMode
{
    [Display(Name = "Simple")] Simple,
    [Display(Name = "Group")] Group,
    [Display(Name = "Programmatic")] Programmatic,
}
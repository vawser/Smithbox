using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

public enum ExportModifier
{
    [Display(Name ="All")]
    None,
    [Display(Name = "Modified Only")]
    ModifiedOnly,
    [Display(Name = "Unique Only")]
    UniqueOnly
}
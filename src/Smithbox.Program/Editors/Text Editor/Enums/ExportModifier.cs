using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

public enum ExportModifier
{
    [Display(Name = "TEXT_ENUM_ExportModifier_All")]
    None,

    [Display(Name = "TEXT_ENUM_ExportModifier_Modified_Only")]
    ModifiedOnly,

    [Display(Name = "TEXT_ENUM_ExportModifier_Unique_Only")]
    UniqueOnly
}
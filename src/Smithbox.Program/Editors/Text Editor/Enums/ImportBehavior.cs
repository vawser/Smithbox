using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

public enum ImportBehavior
{
    // Add entries whose IDs are missing; existing entries are never changed
    [Display(Name = "TEXT_ENUM_ImportBehavior_Append")]
    Append,

    // Add missing entries and update the text of existing ones; nothing is removed
    [Display(Name = "TEXT_ENUM_ImportBehavior_Replace")]
    Replace
}

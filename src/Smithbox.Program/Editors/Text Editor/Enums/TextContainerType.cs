using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

public enum TextContainerType
{
    [Display(Name = "TEXT_ENUM_TextContainerType_Loose")] 
    Loose,

    [Display(Name = "TEXT_ENUM_TextContainerType_BND")] 
    BND
}

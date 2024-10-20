using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

public enum TextContainerType
{
    [Display(Name = "Loose")] Loose,
    [Display(Name = "BND")] BND
}

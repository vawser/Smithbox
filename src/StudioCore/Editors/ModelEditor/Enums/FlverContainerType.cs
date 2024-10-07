using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.ModelEditor.Enums;

public enum FlverContainerType
{
    [Display(Name = "None")] None,
    [Display(Name = "Character")] Character,
    [Display(Name = "Object")] Object,
    [Display(Name = "MapPiece")] MapPiece,
    [Display(Name = "Parts")] Parts,
    [Display(Name = "Loose")] Loose
}

using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MapEditor;

public enum FileSelectionType
{
    [Display(Name = "None")] None,
    [Display(Name = "Character")] Character,
    [Display(Name = "Enemy")] Enemy,
    [Display(Name = "Asset")] Asset,
    [Display(Name = "Part")] Part,
    [Display(Name = "Map Piece")] MapPiece,
    [Display(Name = "Loose")] Loose
}

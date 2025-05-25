using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Enums;

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

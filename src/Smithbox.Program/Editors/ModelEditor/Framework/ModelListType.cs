using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public enum ModelListType
{
    [Display(Name ="Characters")]
    Character,

    [Display(Name = "Assets")]
    Asset,

    [Display(Name = "Parts")]
    Part,

    [Display(Name = "Map Pieces")]
    MapPiece,

    [Display(Name = "Collisions")]
    Collision,

    [Display(Name = "Navmesh")]
    Navmesh
}
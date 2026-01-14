using System.ComponentModel.DataAnnotations;

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
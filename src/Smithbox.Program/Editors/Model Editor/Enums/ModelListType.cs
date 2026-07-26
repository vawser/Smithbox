using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.ModelEditor;

public enum ModelListType
{
    [Display(Name = "MODEL_ENUM_ModelListType_Characters")]
    Character,

    [Display(Name = "MODEL_ENUM_ModelListType_Assets")]
    Asset,

    [Display(Name = "MODEL_ENUM_ModelListType_Parts")]
    Part,

    [Display(Name = "MODEL_ENUM_ModelListType_MapPieces")]
    MapPiece,

    [Display(Name = "MODEL_ENUM_ModelListType_Collisions")]
    Collision,

    [Display(Name = "MODEL_ENUM_ModelListType_Navmesh")]
    Navmesh
}
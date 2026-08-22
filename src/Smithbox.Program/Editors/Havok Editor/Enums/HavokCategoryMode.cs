using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public enum HavokCategoryMode
{
    [Display(Name = "HAVOK_ENUM_HavokCategoryMode_None")]
    None,

    [Display(Name = "HAVOK_ENUM_HavokCategoryMode_Animation")]
    Animation,

    [Display(Name = "HAVOK_ENUM_HavokCategoryMode_Behavior")]
    Behavior,

    [Display(Name = "HAVOK_ENUM_HavokCategoryMode_Character")]
    Character,

    [Display(Name = "HAVOK_ENUM_HavokCategoryMode_Map_Collision")]
    Map_Collision,

    [Display(Name = "HAVOK_ENUM_HavokCategoryMode_Asset_Collision")]
    Asset_Collision,

    //[Display(Name = "HAVOK_ENUM_HavokCategoryMode_Character_Collision")]
    //Character_Collision,

    [Display(Name = "HAVOK_ENUM_HavokCategoryMode_Navmesh")]
    Navmesh,

    [Display(Name = "HAVOK_ENUM_HavokCategoryMode_Cutscene")]
    Cutscene,

    [Display(Name = "HAVOK_ENUM_HavokCategoryMode_Part_Collidable")]
    Part_Collidable,

    [Display(Name = "HAVOK_ENUM_HavokCategoryMode_Rumble")]
    Rumble
}
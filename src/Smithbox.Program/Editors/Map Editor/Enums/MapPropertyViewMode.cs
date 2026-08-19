using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudioCore.Editors.MapEditor;

public enum MapCollisionViewMode
{
    [Display(Name = "MAP_MapPropertyViewMode_MSB")]
    MSB,

    [Display(Name = "MAP_MapPropertyViewMode_CollisionHKX")]
    CollisionHKX
}

public enum MapNavmeshViewMode
{
    [Display(Name = "MAP_MapPropertyViewMode_NVA")]
    NVA,

    [Display(Name = "MAP_MapPropertyViewMode_NavmeshHKX")]
    NavmeshHKX
}
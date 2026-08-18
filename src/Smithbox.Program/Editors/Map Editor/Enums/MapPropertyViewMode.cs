using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudioCore.Editors.MapEditor;

public enum MapPropertyViewMode
{
    [Display(Name = "MAP_MapPropertyViewMode_MSB")]
    MSB,
    [Display(Name = "MAP_MapPropertyViewMode_CollisionHKX")]
    CollisionHKX
}
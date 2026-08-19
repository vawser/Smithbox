using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudioCore.Editors.MapEditor;

public enum MapCollisionEditType
{
    [Display(Name = "MAP_MapCollisionEditType_High")] High,
    [Display(Name = "MAP_MapCollisionEditType_Low")] Low,
    [Display(Name = "MAP_MapCollisionEditType_FallProtection")] FallProtection,
}


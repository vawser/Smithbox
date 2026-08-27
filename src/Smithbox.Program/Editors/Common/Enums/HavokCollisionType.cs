using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.Common;

public enum HavokCollisionType
{
    [Display(Name = "MAP_MapCollisionEditType_Low")]
    Low,
    [Display(Name = "MAP_MapCollisionEditType_High")]
    High,
    [Display(Name = "MAP_MapCollisionEditType_FallProtection")]
    FallProtection
}

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
    [Display(Name="Low")]
    Low,
    [Display(Name = "High")]
    High,
    [Display(Name = "Fall Protection")]
    FallProtection
}

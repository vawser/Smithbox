using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public enum ColorEditDisplayMode
{
    [Display(Name = "GPARAM_ENUM_RGB")]
    RGB,
    [Display(Name = "GPARAM_ENUM_DECIMAL")]
    Decimal,
    [Display(Name = "GPARAM_ENUM_HSV")]
    HSV
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public enum ColorEditDisplayMode
{
    [Display(Name = "RGB")]
    RGB,
    [Display(Name = "Decimal")]
    Decimal,
    [Display(Name = "HSV")]
    HSV
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Enums;

public enum RotationAxis
{
    [Display(Name = "X")] X,
    [Display(Name = "Y")] Y,
    [Display(Name = "Z")] Z
}

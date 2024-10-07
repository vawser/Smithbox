using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Enums;

public enum FlverBinderType
{
    [Display(Name = "None")] None,
    [Display(Name = "BND")] BND,
    [Display(Name = "BXF")] BXF
}
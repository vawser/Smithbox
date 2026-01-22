using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamTooltipMode
{
    [Display(Name = "Click on the Icon")]
    OnIcon,
    [Display(Name = "Hover on the Field Name")]
    OnFieldName
}
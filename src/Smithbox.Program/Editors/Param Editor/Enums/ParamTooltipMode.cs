using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamTooltipMode
{
    [Display(Name = "PARAM_ENUM_Param_Tooltip_Mode_Click")]
    OnIcon,
    [Display(Name = "PARAM_ENUM_Param_Tooltip_Mode_Hover")]
    OnFieldName
}
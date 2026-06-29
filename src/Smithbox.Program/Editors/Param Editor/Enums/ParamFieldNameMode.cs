using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamFieldNameMode
{
    [Display(Name = "PARAM_ENUM_Field_Name_Mode_Source")]
    Source,

    [Display(Name = "PARAM_ENUM_Field_Name_Mode_Community")]
    Community,

    [Display(Name = "PARAM_ENUM_Field_Name_Mode_Source_Community")]
    Source_Community,

    [Display(Name = "PARAM_ENUM_Field_Name_Mode_Community_Source")]
    Community_Source
}

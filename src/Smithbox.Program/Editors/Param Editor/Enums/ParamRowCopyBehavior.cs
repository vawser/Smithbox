using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamRowCopyBehavior
{
    [Display(Name = "PARAM_ENUM_ParamRowCopyBehavior_ID")]
    ID = 0,
    [Display(Name = "PARAM_ENUM_ParamRowCopyBehavior_Name")]
    Name = 1,
    [Display(Name = "PARAM_ENUM_ParamRowCopyBehavior_ID_and_Name")]
    ID_Name = 2
}
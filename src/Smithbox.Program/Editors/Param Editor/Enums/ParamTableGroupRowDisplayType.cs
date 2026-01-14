using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamTableGroupRowDisplayType
{
    [Display(Name = "ID")]
    ID = 0,
    [Display(Name = "None")]
    None = 1
}

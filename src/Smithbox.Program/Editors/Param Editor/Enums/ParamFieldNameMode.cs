using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamFieldNameMode
{
    [Display(Name = "Source")]
    Source,
    [Display(Name = "Community")]
    Community,
    [Display(Name = "Source (Community)")]
    Source_Community,
    [Display(Name = "Community (Source)")]
    Community_Source
}

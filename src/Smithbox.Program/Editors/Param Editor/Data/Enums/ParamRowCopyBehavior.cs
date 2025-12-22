using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamRowCopyBehavior
{
    [Display(Name = "ID")]
    ID = 0,
    [Display(Name = "Name")]
    Name = 1,
    [Display(Name = "ID and Name")]
    ID_Name = 2
}
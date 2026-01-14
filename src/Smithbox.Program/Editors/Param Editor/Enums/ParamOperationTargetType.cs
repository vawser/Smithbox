using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;
public enum ParamOperationTargetType
{
    [Display(Name = "Selected Rows")] SelectedRows,
    [Display(Name = "Selected Param")] SelectedParam,
    [Display(Name = "All Params")] AllParams
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamEditorContext
{
    [Display(Name = "None")] None,
    [Display(Name = "Param List")] ParamList,
    [Display(Name = "Table Group List")] TableGroupList,
    [Display(Name = "Row List")] RowList,
    [Display(Name = "Field List")] FieldList,
}

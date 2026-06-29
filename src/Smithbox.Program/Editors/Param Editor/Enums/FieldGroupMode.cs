using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum FieldLayoutMode
{
    [Display(Name = "PARAM_ENUM_Field_Layout_Mode_Collapsible")]
    Collapsible,
    [Display(Name = "PARAM_ENUM_Field_Layout_Mode_Simple")]
    Header,
    [Display(Name = "PARAM_ENUM_Field_Layout_Mode_Separator")]
    Separator
}
public enum FieldLayoutUnsortedPlacement
{
    [Display(Name = "PARAM_ENUM_Field_Layout_Placement_Unsorted_Top")]
    Top,
    [Display(Name = "PARAM_ENUM_Field_Layout_Placement_Unsorted_Bottom")]
    Bottom
}
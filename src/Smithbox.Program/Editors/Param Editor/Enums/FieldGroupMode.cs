using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum FieldLayoutMode
{
    [Display(Name = "Collapsible Header")]
    Collapsible,
    [Display(Name = "Simple Header")]
    Header,
    [Display(Name = "Separator")]
    Separator
}
public enum FieldLayoutUnsortedPlacement
{
    [Display(Name = "Top")]
    Top,
    [Display(Name = "Bottom")]
    Bottom
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamUpgradeRowGetType
{
    [Display(Name = "All Rows")] AllRows = 0,
    [Display(Name = "Modified Rows")] ModifiedRows = 1,
    [Display(Name = "Selected Rows")] SelectedRows = 2
}

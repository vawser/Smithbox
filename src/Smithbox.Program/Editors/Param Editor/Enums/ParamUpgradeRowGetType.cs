using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamUpgradeRowGetType
{
    [Display(Name = "PARAM_ENUM_ParamUpgradeRowGetType_All")]
    AllRows = 0,

    [Display(Name = "PARAM_ENUM_ParamUpgradeRowGetType_Modified")] 
    ModifiedRows = 1,

    [Display(Name = "PARAM_ENUM_ParamUpgradeRowGetType_Selected")] 
    SelectedRows = 2
}

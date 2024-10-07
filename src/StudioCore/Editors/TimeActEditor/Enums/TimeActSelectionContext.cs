using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Enums;

public enum TimeActSelectionContext
{
    [Display(Name = "None")] None,
    [Display(Name = "File")] File,
    [Display(Name = "Time Act")] TimeAct,
    [Display(Name = "Animation")] Animation,
    [Display(Name = "Event")] Event,
    [Display(Name = "Property")] Property
}

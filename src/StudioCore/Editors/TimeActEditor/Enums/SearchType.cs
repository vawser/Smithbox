using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Enums;

public enum SearchType
{
    [Display(Name = "Animation ID")] AnimationID,
    [Display(Name = "Event ID")] EventID,
    [Display(Name = "Event Name")] EventName,
    [Display(Name = "Event Value")] EventValue,
}
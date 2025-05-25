using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TimeActEditor.Enums;

public enum TimeActOrderType
{
    [Display(Name = "Up")] Up,
    [Display(Name = "Down")] Down,
    [Display(Name = "Top")] Top,
    [Display(Name = "Bottom")] Bottom,
    [Display(Name = "Sort")] Sort
}
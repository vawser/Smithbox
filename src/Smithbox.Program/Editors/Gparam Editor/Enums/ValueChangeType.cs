using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public enum ValueChangeType
{
    [Display(Name = "Set")] Set,
    [Display(Name = "Addition")] Addition,
    [Display(Name = "Subtraction")] Subtraction,
    [Display(Name = "Multiplication")] Multiplication
}
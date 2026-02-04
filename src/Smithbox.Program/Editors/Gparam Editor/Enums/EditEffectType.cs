using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public enum EditEffectType
{
    [Display(Name = "Set")] Set,
    [Display(Name = "Add")] Add,
    [Display(Name = "Subtract")] Subtract,
    [Display(Name = "Multiply")] Multiply,
    [Display(Name = "Set by Row")] SetByRow,
    [Display(Name = "Restore")] Restore,
    [Display(Name = "Random")] Random
}
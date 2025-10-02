using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Enums;
public enum TargetGrid
{
    [Display(Name = "Primary")]
    Primary = 0,
    [Display(Name = "Secondary")]
    Secondary = 1,
    [Display(Name = "Tertiary")]
    Tertiary = 2
}

public enum RootAxis
{
    [Display(Name = "X")]
    X = 0,
    [Display(Name = "Y")]
    Y = 1,
    [Display(Name = "Z")]
    Z = 2
}
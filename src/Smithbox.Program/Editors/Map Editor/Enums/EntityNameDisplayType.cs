using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public enum EntityNameDisplayType
{
    [Display(Name = "Name: Internal")]
    Internal = 0,
    [Display(Name = "Name: Community")]
    Community = 1,
    [Display(Name = "Name: Internal, Alias: Community")]
    Internal_Community = 2,
    [Display(Name = "Name: Internal, Alias: FMG")]
    Internal_FMG = 3,
    [Display(Name = "Name: Community, Alias: FMG")]
    Community_FMG = 4
}


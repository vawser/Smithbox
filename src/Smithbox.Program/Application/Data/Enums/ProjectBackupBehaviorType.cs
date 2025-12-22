using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public enum ProjectBackupBehaviorType
{
    [Display(Name = "None")]
    None = 0,
    [Display(Name = "Simple")]
    Simple = 1,
    [Display(Name = "Complete")]
    Complete = 2
}
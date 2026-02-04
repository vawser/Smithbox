using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public enum ResourceExtractionType
{
    [Display(Name = "Loose")]
    Loose,
    [Display(Name = "Contained")]
    Contained
}


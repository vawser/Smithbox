using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Utilities;

public enum ResourceContainerType
{
    None,
    BND,
    BXF
}
public enum ExtractionType
{
    [Display(Name = "Loose")]
    Loose,
    [Display(Name = "Contained")]
    Contained
}


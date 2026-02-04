using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MaterialEditor;

public enum MaterialListCategory
{
    [Display(Name = "Character")]
    Character,
    [Display(Name = "Part")]
    Part,
    [Display(Name = "Map")]
    Map,
    [Display(Name = "Particles")]
    SFX,
    [Display(Name = "Miscellaneous")]
    Misc,
}
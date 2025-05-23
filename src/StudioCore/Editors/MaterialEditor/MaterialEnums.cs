using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.MaterialEditorNS;

public enum SourceType
{
    [Display(Name = "MTD")]
    MTD,
    [Display(Name = "MATBIN")]
    MATBIN
}


public enum ListCategory
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
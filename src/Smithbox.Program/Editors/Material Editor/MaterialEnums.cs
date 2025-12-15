using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MaterialEditor;

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
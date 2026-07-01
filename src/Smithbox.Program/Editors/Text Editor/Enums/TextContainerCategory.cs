using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

public enum TextContainerCategory
{
    [Display(Name = "TEXT_Language_None")] 
    None,

    [Display(Name = "TEXT_Language_Common")] 
    Common,

    // Languages
    [Display(Name = "TEXT_Language_English")] 
    English,

    [Display(Name = "TEXT_Language_French")] 
    French,

    [Display(Name = "TEXT_Language_German")] 
    German,

    [Display(Name = "TEXT_Language_Italian")] 
    Italian,

    [Display(Name = "TEXT_Language_Japanese")] 
    Japanese,

    [Display(Name = "TEXT_Language_Korean")] 
    Korean,

    [Display(Name = "TEXT_Language_Polish")] 
    Polish,

    [Display(Name = "TEXT_Language_Russian")] 
    Russian,

    [Display(Name = "TEXT_Language_Spanish")] 
    Spanish,

    [Display(Name = "TEXT_Language_TraditionalChinese")] 
    TraditionalChinese,

    [Display(Name = "TEXT_Language_SpanishNeutral")] 
    SpanishNeutral,

    [Display(Name = "TEXT_Language_Portuguese")] 
    Portuguese,

    [Display(Name = "TEXT_Language_SpanishLatin")] 
    SpanishLatin, // Introduced in BB

    [Display(Name = "TEXT_Language_SimplifiedChinese")] 
    SimplifiedChinese, // Introduced in BB

    [Display(Name = "TEXT_Language_Danish")] 
    Danish, // Introduced in BB

    [Display(Name = "TEXT_Language_EnglishUK")] 
    EnglishUK, // Introduced in BB

    [Display(Name = "TEXT_Language_Finnish")] 
    Finnish, // Introduced in BB

    [Display(Name = "TEXT_Language_Dutch")] 
    Dutch, // Introduced in BB

    [Display(Name = "TEXT_Language_Norwegian")] 
    Norwegian, // Introduced in BB

    [Display(Name = "TEXT_Language_PortugueseLatin")] 
    PortugueseLatin,

    [Display(Name = "TEXT_Language_Swedish")] 
    Swedish, // Introduced in BB

    [Display(Name = "TEXT_Language_Turkish")]
    Turkish, // Introduced in BB

    [Display(Name = "TEXT_Language_Thai")] 
    Thai, // Introduced in SDT

    [Display(Name = "TEXT_Language_Arabic")] 
    Arabic, // Intorduced in ER

    // Sell Regions, BB onwards
    [Display(Name = "TEXT_Language_Sell_Region")]
    SellRegion,
}


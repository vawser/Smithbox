using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public enum ProjectType
{
    [Display(Name = "Undefined")]
    Undefined = 0,

    [Display(Name = "Demon's Souls")]
    DES = 1, // Demon's Souls

    [Display(Name = "Dark Souls: Prepare to Die")]
    DS1 = 2, // Dark Souls: Prepare to Die

    [Display(Name = "Dark Souls: Remastered")]
    DS1R = 3, // Dark Souls: Remastered

    [Display(Name = "Dark Souls II: Scholar of the First Sin")]
    DS2S = 4, // Dark Souls II: Scholar of the First Sin

    [Display(Name = "Dark Souls III")]
    DS3 = 5, // Dark Souls III

    [Display(Name = "Bloodborne")]
    BB = 6, // Bloodborne

    [Display(Name = "Sekiro: Shadows Die Twice")]
    SDT = 7, // Sekiro: Shadows Die Twice

    [Display(Name = "Elden Ring")]
    ER = 8, // Elden Ring

    [Display(Name = "Armored Core VI: Fires of Rubicon")]
    AC6 = 9, // Armored Core VI: Fires of Rubicon

    [Display(Name = "Dark Souls II")]
    DS2 = 10, // Dark Souls II

    [Display(Name = "Armored Core 4")]
    AC4 = 11, // Armored Core 4

    [Display(Name = "Armored Core: For Answer")]
    ACFA = 12, // Armored Core: For Answer

    [Display(Name = "Armored Core V")]
    ACV = 13, // Armored Core V

    [Display(Name = "Armored Core: Verdict Day")]
    ACVD = 14, // Armored Core: Verdict Day

    [Display(Name = "Elden Ring: Nightreign")]
    NR = 15, // Elden Ring
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public enum ProjectType
{
    [Display(Name = "PROJECT_Enum_ProjectType_Undefined")]
    Undefined = 0,

    [Display(Name = "PROJECT_Enum_ProjectType_DES")]
    DES = 1, // Demon's Souls

    [Display(Name = "PROJECT_Enum_ProjectType_DS1")]
    DS1 = 2, // Dark Souls: Prepare to Die

    [Display(Name = "PROJECT_Enum_ProjectType_DS1R")]
    DS1R = 3, // Dark Souls: Remastered

    [Display(Name = "PROJECT_Enum_ProjectType_DS2S")]
    DS2S = 4, // Dark Souls II: Scholar of the First Sin

    [Display(Name = "PROJECT_Enum_ProjectType_DS3")]
    DS3 = 5, // Dark Souls III

    [Display(Name = "PROJECT_Enum_ProjectType_BB")]
    BB = 6, // Bloodborne

    [Display(Name = "PROJECT_Enum_ProjectType_SDT")]
    SDT = 7, // Sekiro: Shadows Die Twice

    [Display(Name = "PROJECT_Enum_ProjectType_ER")]
    ER = 8, // Elden Ring

    [Display(Name = "PROJECT_Enum_ProjectType_AC6")]
    AC6 = 9, // Armored Core VI: Fires of Rubicon

    [Display(Name = "PROJECT_Enum_ProjectType_DS2")]
    DS2 = 10, // Dark Souls II

    [Display(Name = "PROJECT_Enum_ProjectType_NR")]
    NR = 15, // Elden Ring: Nightreign
}
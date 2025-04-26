using Andre.Core;
using SoulsFormats;
using System.ComponentModel.DataAnnotations;

namespace StudioCore.Core;

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
    ERN = 15, // Elden Ring
}

public static class ProjectTypeMethods
{
    public static BHD5.Game? AsBhdGame(this ProjectType p)
        => p switch
        {
            ProjectType.DS1 => BHD5.Game.DarkSouls1,
            ProjectType.DS1R => BHD5.Game.DarkSouls1,
            ProjectType.DS2 => BHD5.Game.DarkSouls2,
            ProjectType.DS2S => BHD5.Game.DarkSouls2,
            ProjectType.DS3 => BHD5.Game.DarkSouls3,
            ProjectType.SDT => BHD5.Game.DarkSouls3,
            ProjectType.ER => BHD5.Game.EldenRing,
            _ => null
        };

    public static Game? AsAndreGame(this ProjectType p)
        => p switch
        {
            ProjectType.DES => Game.DES,
            ProjectType.DS1 => Game.DS1,
            ProjectType.DS1R => Game.DS1R,
            ProjectType.DS2S => Game.DS2S,
            ProjectType.DS3 => Game.DS3,
            ProjectType.BB => Game.BB,
            ProjectType.SDT => Game.SDT,
            ProjectType.ER => Game.ER,
            ProjectType.AC6 => Game.AC6,
            ProjectType.DS2 => Game.DS2,
            _ => null
        };

    public static bool IsLooseGame(this ProjectType p)
    {
        switch (p)
        {
            case ProjectType.DES:
            case ProjectType.BB:
                return true;
        }

        return false;
    }
}
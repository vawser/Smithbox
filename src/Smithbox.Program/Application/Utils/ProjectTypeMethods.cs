using Andre.Core;
using SoulsFormats;
using System.ComponentModel.DataAnnotations;

namespace StudioCore.Application;
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
            ProjectType.NR => BHD5.Game.EldenRing,
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
            ProjectType.NR => Game.NR,
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
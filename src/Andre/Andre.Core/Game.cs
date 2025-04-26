using SoulsFormats;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
namespace Andre.Core
{
    public enum Game
    {
        DES, // Demon's Souls
        DS1, // Dark Souls: Prepare to Die
        DS1R, // Dark Souls: Remastered
        DS2S, // Dark Souls II: Scholar of the First Sin
        DS3, // Dark Souls III
        BB, // Bloodborne
        SDT, // Sekiro: Shadows Die Twice
        ER, // Elden Ring
        AC6, // Armored Core VI: Fires of Rubicon
        DS2, // Dark Souls II
        ERN // Elden Ring: Nightreign
    }

    public static class GameMethods
    {
        public static BHD5.Game? AsBhdGame(this Game p)
        {
            return p switch
            {
                Game.DS1 => BHD5.Game.DarkSouls1,
                Game.DS1R => BHD5.Game.DarkSouls1,
                Game.DS2 => BHD5.Game.DarkSouls2,
                Game.DS2S => BHD5.Game.DarkSouls2,
                Game.DS3 => BHD5.Game.DarkSouls3,
                Game.SDT => BHD5.Game.DarkSouls3,
                Game.ER => BHD5.Game.EldenRing,
                Game.AC6 => BHD5.Game.EldenRing,
                Game.BB => null,
                Game.DES => null,
                Game.ERN => BHD5.Game.EldenRing,
                _ => null
            };
        }
    }
}
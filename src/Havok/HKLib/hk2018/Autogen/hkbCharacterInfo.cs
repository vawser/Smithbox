// Automatically Generated

namespace HKLib.hk2018;

public class hkbCharacterInfo : hkReferencedObject
{
    public ulong m_characterId;

    public hkbCharacterInfo.Event m_event;

    public int m_padding;


    public enum Event : int
    {
        REMOVED_FROM_WORLD = 0,
        SHOWN = 1,
        HIDDEN = 2,
        ACTIVATED = 3,
        DEACTIVATED = 4
    }

}


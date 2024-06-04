// Automatically Generated

namespace HKLib.hk2018;

public class hkCommand : IHavokObject
{
    public ushort m_sizePaddedTo16;

    public byte m_filterBits;

    public hkCommand.PrimaryType m_primaryType;

    public ushort m_secondaryType;


    public enum PrimaryType : int
    {
        TYPE_NOP = 0,
        TYPE_DEBUG_DISPLAY = 1,
        TYPE_PHYSICS_API = 2,
        TYPE_PHYSICS_INTERNAL = 3,
        TYPE_PHYSICS_EVENTS = 4,
        TYPE_DESTRUCTION_API = 5,
        TYPE_DESTRUCTION_INTERNAL = 6,
        TYPE_DESTRUCTION_EVENTS = 7,
        TYPE_AI_WORLD_API = 8,
        TYPE_MAX = 9
    }

}


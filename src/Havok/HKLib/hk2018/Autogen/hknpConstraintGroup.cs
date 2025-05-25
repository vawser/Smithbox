// Automatically Generated

namespace HKLib.hk2018;

public class hknpConstraintGroup : IHavokObject
{
    public byte m_microStepMultiplier;

    public hknpConstraintGroupId m_id = new();

    public hknpConstraintId m_firstConstraintId = new();

    public uint m_numConstraintIds;

    public hknpConstraintGroup.FlagsEnum m_flags;

    public ulong m_userData;


    [Flags]
    public enum FlagsEnum : int
    {
        NO_FLAGS = 0,
        IS_FREELIST = 1
    }

}


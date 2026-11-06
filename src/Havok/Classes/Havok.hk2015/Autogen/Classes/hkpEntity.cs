// Automatically Generated

namespace Havok.hk2015;

public class hkpEntity : hkpWorldObject
{
    public hkpMaterial m_material = new();

    public float m_damageMultiplier;

    public ushort m_storageIndex;

    public ushort m_contactPointCallbackDelay;

    public sbyte m_autoRemoveLevel;

    public byte m_numShapeKeysInContactPointProperties;

    public byte m_responseModifierFlags;

    public uint m_uid;

    public hkpEntity.SpuCollisionCallback m_spuCollisionCallback = new();

    public hkpMaxSizeMotion m_motion = new();

    public hkLocalFrame? m_localFrame;


    public class ExtendedListeners : IHavokObject
    {
    }

    public class SmallArraySerializeOverrideType : IHavokObject
    {
        public ushort m_size;

        public ushort m_capacityAndFlags;

    }

    public class SpuCollisionCallback : IHavokObject
    {
        public byte m_eventFilter;

        public byte m_userFilter;

    }

}

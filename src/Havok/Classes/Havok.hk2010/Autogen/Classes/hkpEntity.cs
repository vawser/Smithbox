// Automatically Generated

namespace Havok.hk2010;

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

    public hkpEntitySpuCollisionCallback m_spuCollisionCallback = new();

    public hkpMaxSizeMotion m_motion = new();

    public hkLocalFrame? m_localFrame;

    public uint m_npData;

}

// Automatically Generated

namespace Havok.hk55;

public class hkpCollidableBoundingVolumeData : IHavokObject
{
    public readonly uint[] m_min = new uint[3];

    public readonly byte[] m_expansionMin = new byte[3];

    public byte m_expansionShift;

    public readonly uint[] m_max = new uint[3];

    public readonly byte[] m_expansionMax = new byte[3];

    public byte m_padding;

    public ushort m_numChildShapeAabbs;

    public hkAabbUint32? m_childShapeAabbs;

}

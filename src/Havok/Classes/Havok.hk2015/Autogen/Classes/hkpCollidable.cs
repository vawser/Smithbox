// Automatically Generated

namespace Havok.hk2015;

public class hkpCollidable : hkpCdBody
{
    public byte m_forceCollideOntoPpu;

    public hkpTypedBroadPhaseHandle m_broadPhaseHandle = new();

    public float m_allowedPenetrationDepth;


    public class BoundingVolumeData : IHavokObject
    {
        public readonly uint[] m_min = new uint[3];

        public readonly byte[] m_expansionMin = new byte[3];

        public byte m_expansionShift;

        public readonly uint[] m_max = new uint[3];

        public readonly byte[] m_expansionMax = new byte[3];

    }

}

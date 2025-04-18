// Automatically Generated

namespace HKLib.hk2018;

public class hclAntiPinchConstraintSet : hclConstraintSet
{
    public List<hclAntiPinchConstraintSet.PerParticle> m_perParticleData = new();

    public float m_toAnimPeriod;

    public float m_toSimPeriod;

    public float m_toSimMaxDistance;

    public uint m_referenceMeshBufferIdx;


    public class PerParticle : IHavokObject
    {
        public ushort m_particleIndex;

        public ushort m_referenceVertex;

    }


}


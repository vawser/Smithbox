// Automatically Generated

namespace HKLib.hk2018;

public class hclTransitionConstraintSet : hclConstraintSet
{
    public List<hclTransitionConstraintSet.PerParticle> m_perParticleData = new();

    public float m_toAnimPeriod;

    public float m_toAnimPlusDelayPeriod;

    public float m_toSimPeriod;

    public float m_toSimPlusDelayPeriod;

    public uint m_referenceMeshBufferIdx;


    public class PerParticle : IHavokObject
    {
        public ushort m_particleIndex;

        public ushort m_referenceVertex;

        public float m_toAnimDelay;

        public float m_toSimDelay;

        public float m_toSimMaxDistance;

    }


}


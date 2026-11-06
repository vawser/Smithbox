// Automatically Generated

namespace Havok.hk2014;

public class hclCompressibleLinkConstraintSetMx : hclConstraintSet
{
    public List<hclCompressibleLinkConstraintSetMx.Batch> m_batches = new();

    public List<hclCompressibleLinkConstraintSetMx.Single> m_singles = new();


    public class Batch : IHavokObject
    {
        public readonly float[] m_restLengths = new float[16];

        public readonly float[] m_compressionLengths = new float[16];

        public readonly float[] m_stiffnessesA = new float[16];

        public readonly float[] m_stiffnessesB = new float[16];

        public readonly ushort[] m_particlesA = new ushort[16];

        public readonly ushort[] m_particlesB = new ushort[16];

    }

    public class Single : IHavokObject
    {
        public float m_restLength;

        public float m_compressionLength;

        public float m_stiffnessA;

        public float m_stiffnessB;

        public ushort m_particleA;

        public ushort m_particleB;

    }

}

// Automatically Generated

namespace HKLib.hk2018;

public class hclBendLinkConstraintSetMx : hclConstraintSet
{
    public List<hclBendLinkConstraintSetMx.Batch> m_batches = new();

    public List<hclBendLinkConstraintSetMx.Single> m_singles = new();


    public class Single : IHavokObject
    {
        public float m_bendMinLength;

        public float m_stretchMaxLength;

        public float m_stretchStiffness;

        public float m_bendStiffness;

        public float m_invMassA;

        public float m_invMassB;

        public ushort m_particleA;

        public ushort m_particleB;

    }


    public class Batch : IHavokObject
    {
        public readonly float[] m_bendMinLengths = new float[16];

        public readonly float[] m_stretchMaxLengths = new float[16];

        public readonly float[] m_stretchStiffnesses = new float[16];

        public readonly float[] m_bendStiffnesses = new float[16];

        public readonly float[] m_invMassesA = new float[16];

        public readonly float[] m_invMassesB = new float[16];

        public readonly ushort[] m_particlesA = new ushort[16];

        public readonly ushort[] m_particlesB = new ushort[16];

    }


}


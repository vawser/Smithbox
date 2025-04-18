// Automatically Generated

namespace HKLib.hk2018;

public class hclStretchLinkConstraintSetMx : hclConstraintSet
{
    public List<hclStretchLinkConstraintSetMx.Batch> m_batches = new();

    public List<hclStretchLinkConstraintSetMx.Single> m_singles = new();


    public class Single : IHavokObject
    {
        public float m_restLength;

        public float m_stiffness;

        public uint m_particleA;

        public uint m_particleB;

    }


    public class Batch : IHavokObject
    {
        public readonly float[] m_restLengths = new float[16];

        public readonly float[] m_stiffnesses = new float[16];

        public readonly ushort[] m_particlesA = new ushort[16];

        public readonly ushort[] m_particlesB = new ushort[16];

    }


}


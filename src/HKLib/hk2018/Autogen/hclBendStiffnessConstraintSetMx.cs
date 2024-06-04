// Automatically Generated

namespace HKLib.hk2018;

public class hclBendStiffnessConstraintSetMx : hclConstraintSet
{
    public List<hclBendStiffnessConstraintSetMx.Batch> m_batches = new();

    public List<hclBendStiffnessConstraintSetMx.Single> m_singles = new();

    public float m_maxRestPoseHeightSq;

    public bool m_clampBendStiffness;

    public bool m_useRestPoseConfig;


    public class Single : IHavokObject
    {
        public float m_weightA;

        public float m_weightB;

        public float m_weightC;

        public float m_weightD;

        public float m_bendStiffness;

        public float m_restCurvature;

        public float m_invMassA;

        public float m_invMassB;

        public float m_invMassC;

        public float m_invMassD;

        public ushort m_particleA;

        public ushort m_particleB;

        public ushort m_particleC;

        public ushort m_particleD;

    }


    public class Batch : IHavokObject
    {
        public readonly float[] m_weightsA = new float[16];

        public readonly float[] m_weightsB = new float[16];

        public readonly float[] m_weightsC = new float[16];

        public readonly float[] m_weightsD = new float[16];

        public readonly float[] m_bendStiffnesses = new float[16];

        public readonly float[] m_restCurvatures = new float[16];

        public readonly float[] m_invMassesA = new float[16];

        public readonly float[] m_invMassesB = new float[16];

        public readonly float[] m_invMassesC = new float[16];

        public readonly float[] m_invMassesD = new float[16];

        public readonly ushort[] m_particlesA = new ushort[16];

        public readonly ushort[] m_particlesB = new ushort[16];

        public readonly ushort[] m_particlesC = new ushort[16];

        public readonly ushort[] m_particlesD = new ushort[16];

    }


}


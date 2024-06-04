// Automatically Generated

namespace HKLib.hk2018;

public class hclVolumeConstraintMx : hclConstraintSet
{
    public List<hclVolumeConstraintMx.FrameBatchData> m_frameBatchDatas = new();

    public List<hclVolumeConstraintMx.FrameSingleData> m_frameSingleDatas = new();

    public List<hclVolumeConstraintMx.ApplyBatchData> m_applyBatchDatas = new();

    public List<hclVolumeConstraintMx.ApplySingleData> m_applySingleDatas = new();


    public class ApplySingleData : IHavokObject
    {
        public Vector4 m_frameVector = new();

        public ushort m_particleIndex;

        public float m_stiffness;

    }


    public class ApplyBatchData : IHavokObject
    {
        public readonly Vector4[] m_frameVector = new Vector4[16];

        public readonly ushort[] m_particleIndex = new ushort[16];

        public readonly float[] m_stiffness = new float[16];

    }


    public class FrameSingleData : IHavokObject
    {
        public Vector4 m_frameVector = new();

        public ushort m_particleIndex;

        public float m_weight;

    }


    public class FrameBatchData : IHavokObject
    {
        public readonly Vector4[] m_frameVector = new Vector4[16];

        public readonly ushort[] m_particleIndex = new ushort[16];

        public readonly float[] m_weight = new float[16];

    }


}


// Automatically Generated

namespace HKLib.hk2018;

public class hkaPredictiveCompressedAnimation : hkaAnimation
{
    public List<byte> m_compressedData = new();

    public List<ushort> m_intData = new();

    public readonly int[] m_intArrayOffsets = new int[9];

    public List<float> m_floatData = new();

    public readonly int[] m_floatArrayOffsets = new int[3];

    public int m_numBones;

    public int m_numFloatSlots;

    public int m_numFrames;

    public int m_firstFloatBlockScaleAndOffsetIndex;


    public class TrackCompressionParams : IHavokObject
    {
        public float m_staticTranslationTolerance;

        public float m_staticRotationTolerance;

        public float m_staticScaleTolerance;

        public float m_staticFloatTolerance;

        public float m_dynamicTranslationTolerance;

        public float m_dynamicRotationTolerance;

        public float m_dynamicScaleTolerance;

        public float m_dynamicFloatTolerance;

    }


}


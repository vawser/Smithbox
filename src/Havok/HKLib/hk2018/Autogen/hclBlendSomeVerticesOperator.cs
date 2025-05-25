// Automatically Generated

namespace HKLib.hk2018;

public class hclBlendSomeVerticesOperator : hclOperator
{
    public List<hclBlendSomeVerticesOperator.BlendEntry> m_blendEntries = new();

    public hclBlendSomeVerticesOperator.BlendVertices m_blendVertices = new();

    public hclBlendSomeVerticesOperator.DynamicBlendData m_dynamicBlendData = new();

    public uint m_bufferIdx_A;

    public uint m_bufferIdx_B;

    public uint m_bufferIdx_C;

    public bool m_blendNormals;

    public bool m_blendTangents;

    public bool m_blendBitangents;

    public bool m_dynamicBlend;


    public enum BlendWeightType : int
    {
        CONSTANT_BLEND = 0,
        CUSTOM_WEIGHT = 1,
        BUFFER_A_WEIGHT = 2,
        BUFFER_B_WEIGHT = 3,
        BLEND_BUFFER_A_TO_B = 4,
        BLEND_BUFFER_B_TO_A = 5,
        BLEND_BUFFER_A_TO_CUSTOM_WEIGHT = 6,
        BLEND_BUFFER_B_TO_CUSTOM_WEIGHT = 7,
        BLEND_CUSTOM_WEIGHT_TO_BUFFER_A = 8,
        BLEND_CUSTOM_WEIGHT_TO_BUFFER_B = 9
    }

    public class DynamicBlendData : IHavokObject
    {
        public float m_defaultWeight;

        public float m_transitionPeriod;

        public bool m_mapToSCurve;

    }


    public class BlendVertices : IHavokObject
    {
        public List<ushort> m_vertexIndices = new();

        public float m_constBlendWeight;

    }


    public class BlendEntry : IHavokObject
    {
        public ushort m_vertexIndex;

        public float m_blendWeight;

    }


}


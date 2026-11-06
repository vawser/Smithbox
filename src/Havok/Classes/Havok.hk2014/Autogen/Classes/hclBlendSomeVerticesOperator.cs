// Automatically Generated

namespace Havok.hk2014;

public class hclBlendSomeVerticesOperator : hclOperator
{
    public List<hclBlendSomeVerticesOperator.BlendEntry> m_blendEntries = new();

    public uint m_bufferIdx_A;

    public uint m_bufferIdx_B;

    public uint m_bufferIdx_C;

    public bool m_blendNormals;

    public bool m_blendTangents;

    public bool m_blendBitangents;


    public class BlendEntry : IHavokObject
    {
        public ushort m_vertexIndex;

        public float m_blendWeight;

    }

}

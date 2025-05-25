// Automatically Generated

namespace HKLib.hk2018;

public class hclGatherSomeVerticesOperator : hclOperator
{
    public List<hclGatherSomeVerticesOperator.VertexPair> m_vertexPairs = new();

    public uint m_inputBufferIdx;

    public uint m_outputBufferIdx;

    public bool m_gatherNormals;


    public class VertexPair : IHavokObject
    {
        public ushort m_indexInput;

        public ushort m_indexOutput;

    }


}


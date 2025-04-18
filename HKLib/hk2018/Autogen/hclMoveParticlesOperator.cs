// Automatically Generated

namespace HKLib.hk2018;

public class hclMoveParticlesOperator : hclOperator
{
    public List<hclMoveParticlesOperator.VertexParticlePair> m_vertexParticlePairs = new();

    public uint m_simClothIndex;

    public uint m_refBufferIdx;


    public class VertexParticlePair : IHavokObject
    {
        public ushort m_vertexIndex;

        public ushort m_particleIndex;

    }


}


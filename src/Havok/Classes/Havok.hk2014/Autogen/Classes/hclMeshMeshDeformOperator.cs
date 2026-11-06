// Automatically Generated

namespace Havok.hk2014;

public class hclMeshMeshDeformOperator : hclOperator
{
    public List<ushort> m_inputTrianglesSubset = new();

    public List<hclMeshMeshDeformOperator.TriangleVertexPair> m_triangleVertexPairs = new();

    public List<ushort> m_triangleVertexStartForVertex = new();

    public uint m_inputBufferIdx;

    public uint m_outputBufferIdx;

    public ushort m_startVertex;

    public ushort m_endVertex;

    public hclMeshMeshDeformOperator.ScaleNormalBehaviour m_scaleNormalBehaviour;

    public bool m_deformNormals;

    public bool m_partialDeform;


    public class TriangleVertexPair : IHavokObject
    {
        public Vector4 m_localPosition = new();

        public Vector4 m_localNormal = new();

        public ushort m_triangleIndex;

        public float m_weight;

    }

    public enum ScaleNormalBehaviour : int
    {
        SCALE_NORMAL_IGNORE = 0,
        SCALE_NORMAL_APPLY = 1,
        SCALE_NORMAL_INVERT = 2
    }

}

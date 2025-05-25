// Automatically Generated

namespace HKLib.hk2018;

public class hclUpdateAllVertexFramesOperator : hclOperator
{
    public List<ushort> m_vertToNormalID = new();

    public List<byte> m_triangleFlips = new();

    public List<ushort> m_referenceVertices = new();

    public List<float> m_tangentEdgeCosAngle = new();

    public List<float> m_tangentEdgeSinAngle = new();

    public List<float> m_biTangentFlip = new();

    public uint m_bufferIdx;

    public uint m_numUniqueNormalIDs;

    public bool m_updateNormals;

    public bool m_updateTangents;

    public bool m_updateBiTangents;

}


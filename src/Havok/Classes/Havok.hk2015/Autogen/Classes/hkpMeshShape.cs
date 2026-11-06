// Automatically Generated

namespace Havok.hk2015;

public class hkpMeshShape : hkpShapeCollection
{
    public Vector4 m_scaling = new();

    public int m_numBitsForSubpartIndex;

    public List<hkpMeshShape.Subpart> m_subparts = new();

    public List<ushort> m_weldingInfo = new();

    public byte m_weldingType;

    public float m_radius;

    public readonly int[] m_pad = new int[3];


    public class Subpart : IHavokObject
    {
        public int m_vertexStriding;

        public int m_numVertices;

        public sbyte m_stridingType;

        public sbyte m_materialIndexStridingType;

        public int m_indexStriding;

        public int m_flipAlternateTriangles;

        public int m_numTriangles;

        public int m_materialIndexStriding;

        public int m_materialStriding;

        public int m_numMaterials;

        public int m_triangleOffset;

    }

}

// Automatically Generated

namespace Havok.hk2015;

public class hkpSimpleMeshShape : hkpShapeCollection
{
    public List<Vector4> m_vertices = new();

    public List<hkpSimpleMeshShape.Triangle> m_triangles = new();

    public List<byte> m_materialIndices = new();

    public float m_radius;

    public byte m_weldingType;


    public class Triangle : IHavokObject
    {
        public int m_a;

        public int m_b;

        public int m_c;

        public ushort m_weldingInfo;

    }

}

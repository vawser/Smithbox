// Automatically Generated

namespace Havok.hk2014;

public class hkGeometry : hkReferencedObject
{
    public List<Vector4> m_vertices = new();

    public List<hkGeometry.Triangle> m_triangles = new();


    public class Triangle : IHavokObject
    {
        public int m_a;

        public int m_b;

        public int m_c;

        public int m_material;

    }

}

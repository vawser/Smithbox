// Automatically Generated

namespace HKLib.hk2018;

public class hknpExtendedExternMeshShapeGeometry : hknpExternMeshShapeGeometry
{
    public List<hknpExtendedExternMeshShapeGeometry.Triangle> m_triangles = new();

    public List<hknpExtendedExternMeshShapeGeometry.Quad> m_quads = new();


    public class Quad : IHavokObject
    {
        public readonly Vector4[] m_vertices = new Vector4[4];

    }


    public class Triangle : IHavokObject
    {
        public readonly Vector4[] m_vertices = new Vector4[3];

    }


}


// Automatically Generated

namespace Havok.hk2015;

public class hkpExtendedMeshShape : hkpShapeCollection
{
    public hkpExtendedMeshShape.TrianglesSubpart m_embeddedTrianglesSubpart = new();

    public Vector4 m_aabbHalfExtents = new();

    public Vector4 m_aabbCenter = new();

    public int m_numBitsForSubpartIndex;

    public List<hkpExtendedMeshShape.TrianglesSubpart> m_trianglesSubparts = new();

    public List<hkpExtendedMeshShape.ShapesSubpart> m_shapesSubparts = new();

    public List<ushort> m_weldingInfo = new();

    public byte m_weldingType;

    public uint m_defaultCollisionFilterInfo;

    public int m_cachedNumChildShapes;

    public float m_triangleRadius;


    public class ShapesSubpart : hkpExtendedMeshShape.Subpart
    {
        public List<hkpConvexShape?> m_childShapes = new();

        public Quaternion m_rotation = new();

        public Vector4 m_translation = new();

    }

    public class Subpart : IHavokObject
    {
        public ushort m_typeAndFlags;

        public ushort m_shapeInfo;

        public ushort m_materialIndexStriding;

        public ulong m_userData;

    }

    public class TrianglesSubpart : hkpExtendedMeshShape.Subpart
    {
        public int m_numTriangleShapes;

        public int m_numVertices;

        public ushort m_vertexStriding;

        public int m_triangleOffset;

        public ushort m_indexStriding;

        public sbyte m_stridingType;

        public sbyte m_flipAlternateTriangles;

        public Vector4 m_extrusion = new();

        public hkQsTransform m_transform = new();

    }

}

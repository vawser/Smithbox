// Automatically Generated

namespace Havok.hk55;

public class hkpExtendedMeshShape : hkpShapeCollection
{
    public int m_numBitsForSubpartIndex;

    public Vector4 m_scaling = new();

    public Vector4 m_aabbHalfExtents = new();

    public Vector4 m_aabbCenter = new();

    public List<hkpExtendedMeshShapeTrianglesSubpart> m_trianglesSubparts = new();

    public List<hkpExtendedMeshShapeShapesSubpart> m_shapesSubparts = new();

    public List<ushort> m_weldingInfo = new();

    public byte m_weldingType;

    public hkpExtendedMeshShapeTrianglesSubpart m_embeddedTrianglesSubpart = new();

    public uint m_defaultCollisionFilterInfo;

    public int m_cachedNumChildShapes;

    public float m_triangleRadius;

    public int m_padding;


    public enum SubpartType : int
    {
        SUBPART_TRIANGLES = 0,
        SUBPART_SHAPE = 1
    }

    public enum MaterialIndexStridingType : int
    {
        MATERIAL_INDICES_INVALID = 0,
        MATERIAL_INDICES_INT8 = 1,
        MATERIAL_INDICES_INT16 = 2,
        MATERIAL_INDICES_MAX_ID = 3
    }

    public enum IndexStridingType : int
    {
        INDICES_INVALID = 0,
        INDICES_INT16 = 1,
        INDICES_INT32 = 2,
        INDICES_MAX_ID = 3
    }

}

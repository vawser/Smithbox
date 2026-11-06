// Automatically Generated

namespace Havok.hk2014;

public class hkpExtendedMeshShape : hkpShapeCollection
{
    public hkpExtendedMeshShapeTrianglesSubpart m_embeddedTrianglesSubpart = new();

    public Vector4 m_aabbHalfExtents = new();

    public Vector4 m_aabbCenter = new();

    public int m_numBitsForSubpartIndex;

    public List<hkpExtendedMeshShapeTrianglesSubpart> m_trianglesSubparts = new();

    public List<hkpExtendedMeshShapeShapesSubpart> m_shapesSubparts = new();

    public List<ushort> m_weldingInfo = new();

    public hkpWeldingUtility.WeldingType m_weldingType;

    public uint m_defaultCollisionFilterInfo;

    public int m_cachedNumChildShapes;

    public float m_triangleRadius;


    public enum IndexStridingType : int
    {
        INDICES_INVALID = 0,
        INDICES_INT8 = 1,
        INDICES_INT16 = 2,
        INDICES_INT32 = 3,
        INDICES_MAX_ID = 4
    }

}

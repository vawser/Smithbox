// Automatically Generated

namespace Havok.hk2014;

public class hkpMeshShape : hkpShapeCollection
{
    public Vector4 m_scaling = new();

    public int m_numBitsForSubpartIndex;

    public List<hkpMeshShapeSubpart> m_subparts = new();

    public List<ushort> m_weldingInfo = new();

    public hkpWeldingUtility.WeldingType m_weldingType;

    public float m_radius;

    public readonly int[] m_pad = new int[3];


    public enum MeshShapeIndexStridingType : int
    {
        INDICES_INVALID = 0,
        INDICES_INT16 = 1,
        INDICES_INT32 = 2,
        INDICES_MAX_ID = 3
    }

    public enum MeshShapeMaterialIndexStridingType : int
    {
        MATERIAL_INDICES_INVALID = 0,
        MATERIAL_INDICES_INT8 = 1,
        MATERIAL_INDICES_INT16 = 2,
        MATERIAL_INDICES_MAX_ID = 3
    }

}

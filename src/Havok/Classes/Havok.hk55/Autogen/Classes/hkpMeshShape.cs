// Automatically Generated

namespace Havok.hk55;

public class hkpMeshShape : hkpShapeCollection
{
    public Vector4 m_scaling = new();

    public int m_numBitsForSubpartIndex;

    public List<hkpMeshShapeSubpart> m_subparts = new();

    public List<ushort> m_weldingInfo = new();

    public byte m_weldingType;

    public float m_radius;

    public readonly int[] m_pad = new int[3];


    public enum IndexStridingType : int
    {
        INDICES_INVALID = 0,
        INDICES_INT16 = 1,
        INDICES_INT32 = 2,
        INDICES_MAX_ID = 3
    }

}

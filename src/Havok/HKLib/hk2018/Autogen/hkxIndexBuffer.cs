// Automatically Generated

namespace HKLib.hk2018;

public class hkxIndexBuffer : hkReferencedObject
{
    public hkxIndexBuffer.IndexType m_indexType;

    public List<ushort> m_indices16 = new();

    public List<uint> m_indices32 = new();

    public uint m_vertexBaseOffset;

    public uint m_length;


    public enum IndexType : int
    {
        INDEX_TYPE_INVALID = 0,
        INDEX_TYPE_TRI_LIST = 1,
        INDEX_TYPE_TRI_STRIP = 2,
        INDEX_TYPE_TRI_FAN = 3,
        INDEX_TYPE_MAX_ID = 4
    }

}


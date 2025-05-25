// Automatically Generated

namespace HKLib.hk2018;

public class hkxVertexDescription : IHavokObject
{
    public List<hkxVertexDescription.ElementDecl> m_decls = new();


    public enum DataUsage : int
    {
        HKX_DU_NONE = 0,
        HKX_DU_POSITION = 1,
        HKX_DU_COLOR = 2,
        HKX_DU_NORMAL = 4,
        HKX_DU_TANGENT = 8,
        HKX_DU_BINORMAL = 16,
        HKX_DU_TEXCOORD = 32,
        HKX_DU_BLENDWEIGHTS = 64,
        HKX_DU_BLENDINDICES = 128,
        HKX_DU_USERDATA = 256
    }

    public enum DataType : int
    {
        HKX_DT_NONE = 0,
        HKX_DT_UINT8 = 1,
        HKX_DT_INT16 = 2,
        HKX_DT_UINT32 = 3,
        HKX_DT_FLOAT = 4
    }

    public class ElementDecl : IHavokObject
    {
        public uint m_byteOffset;

        public hkxVertexDescription.DataType m_type;

        public hkxVertexDescription.DataUsage m_usage;

        public uint m_byteStride;

        public byte m_numElements;

        public string? m_channelID;

    }


}


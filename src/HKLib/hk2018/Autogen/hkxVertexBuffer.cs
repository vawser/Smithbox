// Automatically Generated

namespace HKLib.hk2018;

public class hkxVertexBuffer : hkReferencedObject
{
    public hkxVertexBuffer.VertexData m_data = new();

    public hkxVertexDescription m_desc = new();


    public class VertexData : IHavokObject
    {
        public List<uint> m_vectorData = new();

        public List<uint> m_floatData = new();

        public List<uint> m_uint32Data = new();

        public List<ushort> m_uint16Data = new();

        public List<byte> m_uint8Data = new();

        public uint m_numVerts;

        public uint m_vectorStride;

        public uint m_floatStride;

        public uint m_uint32Stride;

        public uint m_uint16Stride;

        public uint m_uint8Stride;

    }


}


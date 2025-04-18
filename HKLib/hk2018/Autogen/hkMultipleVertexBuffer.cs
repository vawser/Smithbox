// Automatically Generated

namespace HKLib.hk2018;

public class hkMultipleVertexBuffer : hkMeshVertexBuffer
{
    public hkVertexFormat m_vertexFormat = new();

    public List<hkMultipleVertexBuffer.LockedElement> m_lockedElements = new();

    public hkMemoryMeshVertexBuffer? m_lockedBuffer;

    public List<hkMultipleVertexBuffer.ElementInfo> m_elementInfos = new();

    public List<hkMultipleVertexBuffer.VertexBufferInfo> m_vertexBufferInfos = new();

    public int m_numVertices;

    public bool m_isLocked;

    public uint m_updateCount;

    public bool m_writeLock;

    public bool m_isSharable;

    public bool m_constructionComplete;


    public class LockedElement : IHavokObject
    {
        public byte m_vertexBufferIndex;

        public byte m_elementIndex;

        public byte m_lockedBufferIndex;

        public byte m_vertexFormatIndex;

        public byte m_lockFlags;

        public byte m_outputBufferIndex;

        public sbyte m_emulatedIndex;

    }


    public class ElementInfo : IHavokObject
    {
        public byte m_vertexBufferIndex;

        public byte m_elementIndex;

    }


    public class VertexBufferInfo : IHavokObject
    {
        public hkMeshVertexBuffer? m_vertexBuffer;

        public bool m_isLocked;

    }


}


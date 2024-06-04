// Automatically Generated

namespace HKLib.hk2018;

public class hclBufferLayout : IHavokObject
{
    public readonly hclBufferLayout.BufferElement[] m_elementsLayout = new hclBufferLayout.BufferElement[4];

    public readonly hclBufferLayout.Slot[] m_slots = new hclBufferLayout.Slot[4];

    public byte m_numSlots;

    public hclBufferLayout.TriangleFormat m_triangleFormat;


    public enum TriangleFormat : int
    {
        TF_THREE_INT32S = 0,
        TF_THREE_INT16S = 1,
        TF_OTHER = 2
    }

    public enum SlotFlags : int
    {
        SF_NO_ALIGNED_START = 0,
        SF_16BYTE_ALIGNED_START = 1,
        SF_64BYTE_ALIGNED_START = 3
    }

    public class Slot : IHavokObject
    {
        public hclBufferLayout.SlotFlags m_flags;

        public byte m_stride;

    }


    public class BufferElement : IHavokObject
    {
        public hclRuntimeConversionInfo.VectorConversion m_vectorConversion;

        public byte m_vectorSize;

        public byte m_slotId;

        public byte m_slotStart;

    }


}


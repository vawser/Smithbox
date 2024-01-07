using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum SlotFlags
    {
        SF_NO_ALIGNED_START = 0,
        SF_16BYTE_ALIGNED_START = 1,
        SF_64BYTE_ALIGNED_START = 3,
    }
    
    public enum TriangleFormat
    {
        TF_THREE_INT32S = 0,
        TF_THREE_INT16S = 1,
        TF_OTHER = 2,
    }
    
    public partial class hclBufferLayout : IHavokObject
    {
        public virtual uint Signature { get => 3530040743; }
        
        public hclBufferLayoutBufferElement m_elementsLayout_0;
        public hclBufferLayoutBufferElement m_elementsLayout_1;
        public hclBufferLayoutBufferElement m_elementsLayout_2;
        public hclBufferLayoutBufferElement m_elementsLayout_3;
        public hclBufferLayoutSlot m_slots_0;
        public hclBufferLayoutSlot m_slots_1;
        public hclBufferLayoutSlot m_slots_2;
        public hclBufferLayoutSlot m_slots_3;
        public byte m_numSlots;
        public TriangleFormat m_triangleFormat;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_elementsLayout_0 = new hclBufferLayoutBufferElement();
            m_elementsLayout_0.Read(des, br);
            m_elementsLayout_1 = new hclBufferLayoutBufferElement();
            m_elementsLayout_1.Read(des, br);
            m_elementsLayout_2 = new hclBufferLayoutBufferElement();
            m_elementsLayout_2.Read(des, br);
            m_elementsLayout_3 = new hclBufferLayoutBufferElement();
            m_elementsLayout_3.Read(des, br);
            m_slots_0 = new hclBufferLayoutSlot();
            m_slots_0.Read(des, br);
            m_slots_1 = new hclBufferLayoutSlot();
            m_slots_1.Read(des, br);
            m_slots_2 = new hclBufferLayoutSlot();
            m_slots_2.Read(des, br);
            m_slots_3 = new hclBufferLayoutSlot();
            m_slots_3.Read(des, br);
            m_numSlots = br.ReadByte();
            m_triangleFormat = (TriangleFormat)br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_elementsLayout_0.Write(s, bw);
            m_elementsLayout_1.Write(s, bw);
            m_elementsLayout_2.Write(s, bw);
            m_elementsLayout_3.Write(s, bw);
            m_slots_0.Write(s, bw);
            m_slots_1.Write(s, bw);
            m_slots_2.Write(s, bw);
            m_slots_3.Write(s, bw);
            bw.WriteByte(m_numSlots);
            bw.WriteByte((byte)m_triangleFormat);
        }
    }
}

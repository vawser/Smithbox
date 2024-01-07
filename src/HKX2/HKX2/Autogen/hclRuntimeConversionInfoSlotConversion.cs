using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclRuntimeConversionInfoSlotConversion : IHavokObject
    {
        public virtual uint Signature { get => 3638271097; }
        
        public byte m_elements_0;
        public byte m_elements_1;
        public byte m_elements_2;
        public byte m_elements_3;
        public byte m_numElements;
        public byte m_index;
        public bool m_partialWrite;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_elements_0 = br.ReadByte();
            m_elements_1 = br.ReadByte();
            m_elements_2 = br.ReadByte();
            m_elements_3 = br.ReadByte();
            m_numElements = br.ReadByte();
            m_index = br.ReadByte();
            m_partialWrite = br.ReadBoolean();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte(m_elements_0);
            bw.WriteByte(m_elements_1);
            bw.WriteByte(m_elements_2);
            bw.WriteByte(m_elements_3);
            bw.WriteByte(m_numElements);
            bw.WriteByte(m_index);
            bw.WriteBoolean(m_partialWrite);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclRuntimeConversionInfoElementConversion : IHavokObject
    {
        public virtual uint Signature { get => 2711241786; }
        
        public byte m_index;
        public byte m_offset;
        public VectorConversion m_conversion;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_index = br.ReadByte();
            m_offset = br.ReadByte();
            m_conversion = (VectorConversion)br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte(m_index);
            bw.WriteByte(m_offset);
            bw.WriteByte((byte)m_conversion);
        }
    }
}

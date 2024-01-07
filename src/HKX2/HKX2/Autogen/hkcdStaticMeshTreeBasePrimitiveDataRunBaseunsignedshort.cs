using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticMeshTreeBasePrimitiveDataRunBaseunsignedshort : IHavokObject
    {
        public virtual uint Signature { get => 3672981844; }
        
        public ushort m_value;
        public byte m_index;
        public byte m_count;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_value = br.ReadUInt16();
            m_index = br.ReadByte();
            m_count = br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_value);
            bw.WriteByte(m_index);
            bw.WriteByte(m_count);
        }
    }
}

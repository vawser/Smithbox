using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticMeshTreeBasePrimitiveDataRunBaseunsignedint : IHavokObject
    {
        public virtual uint Signature { get => 4133272332; }
        
        public uint m_value;
        public byte m_index;
        public byte m_count;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_value = br.ReadUInt32();
            m_index = br.ReadByte();
            m_count = br.ReadByte();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_value);
            bw.WriteByte(m_index);
            bw.WriteByte(m_count);
            bw.WriteUInt16(0);
        }
    }
}

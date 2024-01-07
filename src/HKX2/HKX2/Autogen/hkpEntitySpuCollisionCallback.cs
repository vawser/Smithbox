using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpEntitySpuCollisionCallback : IHavokObject
    {
        public virtual uint Signature { get => 2165604101; }
        
        public byte m_eventFilter;
        public byte m_userFilter;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            br.ReadUInt64();
            br.ReadUInt16();
            m_eventFilter = br.ReadByte();
            m_userFilter = br.ReadByte();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
            bw.WriteByte(m_eventFilter);
            bw.WriteByte(m_userFilter);
            bw.WriteUInt32(0);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxVertexAnimationUsageMap : IHavokObject
    {
        public virtual uint Signature { get => 1190729358; }
        
        public DataUsage m_use;
        public byte m_useIndexOrig;
        public byte m_useIndexLocal;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_use = (DataUsage)br.ReadUInt16();
            m_useIndexOrig = br.ReadByte();
            m_useIndexLocal = br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16((ushort)m_use);
            bw.WriteByte(m_useIndexOrig);
            bw.WriteByte(m_useIndexLocal);
        }
    }
}

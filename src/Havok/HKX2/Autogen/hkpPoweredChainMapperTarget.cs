using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpPoweredChainMapperTarget : IHavokObject
    {
        public virtual uint Signature { get => 4132554573; }
        
        public hkpPoweredChainData m_chain;
        public int m_infoIndex;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_chain = des.ReadClassPointer<hkpPoweredChainData>(br);
            m_infoIndex = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkpPoweredChainData>(bw, m_chain);
            bw.WriteInt32(m_infoIndex);
            bw.WriteUInt32(0);
        }
    }
}

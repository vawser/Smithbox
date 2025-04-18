using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpMinMaxQuadTreeMinMaxLevel : IHavokObject
    {
        public virtual uint Signature { get => 2013793389; }
        
        public List<uint> m_minMaxData;
        public ushort m_xRes;
        public ushort m_zRes;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_minMaxData = des.ReadUInt32Array(br);
            m_xRes = br.ReadUInt16();
            m_zRes = br.ReadUInt16();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteUInt32Array(bw, m_minMaxData);
            bw.WriteUInt16(m_xRes);
            bw.WriteUInt16(m_zRes);
            bw.WriteUInt32(0);
        }
    }
}

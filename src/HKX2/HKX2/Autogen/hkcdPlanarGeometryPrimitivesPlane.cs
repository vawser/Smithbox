using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdPlanarGeometryPrimitivesPlane : IHavokObject
    {
        public virtual uint Signature { get => 4194138308; }
        
        public long m_iEqn_0;
        public long m_iEqn_1;
        public long m_iEqn_2;
        public long m_iEqn_3;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_iEqn_0 = br.ReadInt64();
            m_iEqn_1 = br.ReadInt64();
            m_iEqn_2 = br.ReadInt64();
            m_iEqn_3 = br.ReadInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt64(m_iEqn_0);
            bw.WriteInt64(m_iEqn_1);
            bw.WriteInt64(m_iEqn_2);
            bw.WriteInt64(m_iEqn_3);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}

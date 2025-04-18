using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpConstraintChainInstance : hkpConstraintInstance
    {
        public override uint Signature { get => 2590709585; }
        
        public List<hkpEntity> m_chainedEntities;
        public hkpConstraintChainInstanceAction m_action;
        public ulong m_chainConnectedness;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_chainedEntities = des.ReadClassPointerArray<hkpEntity>(br);
            m_action = des.ReadClassPointer<hkpConstraintChainInstanceAction>(br);
            m_chainConnectedness = br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkpEntity>(bw, m_chainedEntities);
            s.WriteClassPointer<hkpConstraintChainInstanceAction>(bw, m_action);
            bw.WriteUInt64(m_chainConnectedness);
        }
    }
}

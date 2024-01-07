using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpPoweredChainMapperLinkInfo : IHavokObject
    {
        public virtual uint Signature { get => 3473349147; }
        
        public int m_firstTargetIdx;
        public int m_numTargets;
        public hkpConstraintInstance m_limitConstraint;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_firstTargetIdx = br.ReadInt32();
            m_numTargets = br.ReadInt32();
            m_limitConstraint = des.ReadClassPointer<hkpConstraintInstance>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_firstTargetIdx);
            bw.WriteInt32(m_numTargets);
            s.WriteClassPointer<hkpConstraintInstance>(bw, m_limitConstraint);
        }
    }
}

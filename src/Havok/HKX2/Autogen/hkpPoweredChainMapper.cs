using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpPoweredChainMapper : hkReferencedObject
    {
        public override uint Signature { get => 3365603871; }
        
        public List<hkpPoweredChainMapperLinkInfo> m_links;
        public List<hkpPoweredChainMapperTarget> m_targets;
        public List<hkpConstraintChainInstance> m_chains;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_links = des.ReadClassArray<hkpPoweredChainMapperLinkInfo>(br);
            m_targets = des.ReadClassArray<hkpPoweredChainMapperTarget>(br);
            m_chains = des.ReadClassPointerArray<hkpConstraintChainInstance>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkpPoweredChainMapperLinkInfo>(bw, m_links);
            s.WriteClassArray<hkpPoweredChainMapperTarget>(bw, m_targets);
            s.WriteClassPointerArray<hkpConstraintChainInstance>(bw, m_chains);
        }
    }
}

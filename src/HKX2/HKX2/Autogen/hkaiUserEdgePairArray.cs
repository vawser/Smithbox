using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiUserEdgePairArray : hkReferencedObject
    {
        public override uint Signature { get => 2084981375; }
        
        public List<hkaiUserEdgeUtilsUserEdgePair> m_edgePairs;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_edgePairs = des.ReadClassArray<hkaiUserEdgeUtilsUserEdgePair>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkaiUserEdgeUtilsUserEdgePair>(bw, m_edgePairs);
        }
    }
}

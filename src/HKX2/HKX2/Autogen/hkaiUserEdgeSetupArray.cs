using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiUserEdgeSetupArray : hkReferencedObject
    {
        public override uint Signature { get => 55940824; }
        
        public List<hkaiUserEdgeUtilsUserEdgeSetup> m_edgeSetups;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_edgeSetups = des.ReadClassArray<hkaiUserEdgeUtilsUserEdgeSetup>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkaiUserEdgeUtilsUserEdgeSetup>(bw, m_edgeSetups);
        }
    }
}

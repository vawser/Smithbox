using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiTraversalAnalysisInput : hkReferencedObject
    {
        public override uint Signature { get => 3654283239; }
        
        public List<hkaiTraversalAnalysisInputSection> m_sections;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_sections = des.ReadClassArray<hkaiTraversalAnalysisInputSection>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkaiTraversalAnalysisInputSection>(bw, m_sections);
        }
    }
}

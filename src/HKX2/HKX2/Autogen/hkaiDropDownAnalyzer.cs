using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiDropDownAnalyzer : hkaiTraversalAnalyzer
    {
        public override uint Signature { get => 1877069901; }
        
        public float m_minDropDistance;
        public float m_maxDropDistance;
        public float m_maxUnderhang;
        public float m_verticalLipHeight;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_minDropDistance = br.ReadSingle();
            m_maxDropDistance = br.ReadSingle();
            m_maxUnderhang = br.ReadSingle();
            m_verticalLipHeight = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_minDropDistance);
            bw.WriteSingle(m_maxDropDistance);
            bw.WriteSingle(m_maxUnderhang);
            bw.WriteSingle(m_verticalLipHeight);
        }
    }
}

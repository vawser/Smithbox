using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiClimbUpAnalyzer : hkaiTraversalAnalyzer
    {
        public override uint Signature { get => 3687978068; }
        
        public float m_maxUnderhang;
        public float m_minUpHeight;
        public float m_maxUpHeight;
        public float m_grabAngle;
        public float m_grabScanDepth;
        public float m_verticalLipHeight;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_maxUnderhang = br.ReadSingle();
            m_minUpHeight = br.ReadSingle();
            m_maxUpHeight = br.ReadSingle();
            m_grabAngle = br.ReadSingle();
            m_grabScanDepth = br.ReadSingle();
            m_verticalLipHeight = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_maxUnderhang);
            bw.WriteSingle(m_minUpHeight);
            bw.WriteSingle(m_maxUpHeight);
            bw.WriteSingle(m_grabAngle);
            bw.WriteSingle(m_grabScanDepth);
            bw.WriteSingle(m_verticalLipHeight);
        }
    }
}

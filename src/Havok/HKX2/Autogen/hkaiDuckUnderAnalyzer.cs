using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiDuckUnderAnalyzer : hkaiTraversalAnalyzer
    {
        public override uint Signature { get => 2383458190; }
        
        public float m_maxHorizontalDistance;
        public float m_minClearance;
        public float m_maxClearance;
        public float m_maxHeightDifference;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_maxHorizontalDistance = br.ReadSingle();
            m_minClearance = br.ReadSingle();
            m_maxClearance = br.ReadSingle();
            m_maxHeightDifference = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_maxHorizontalDistance);
            bw.WriteSingle(m_minClearance);
            bw.WriteSingle(m_maxClearance);
            bw.WriteSingle(m_maxHeightDifference);
        }
    }
}

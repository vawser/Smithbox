using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiJumpAnalyzer : hkaiTraversalAnalyzer
    {
        public override uint Signature { get => 2753075461; }
        
        public float m_maxHorizontalDistance;
        public float m_maxUpHeight;
        public float m_maxDownHeight;
        public float m_verticalApex;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_maxHorizontalDistance = br.ReadSingle();
            m_maxUpHeight = br.ReadSingle();
            m_maxDownHeight = br.ReadSingle();
            m_verticalApex = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_maxHorizontalDistance);
            bw.WriteSingle(m_maxUpHeight);
            bw.WriteSingle(m_maxDownHeight);
            bw.WriteSingle(m_verticalApex);
        }
    }
}

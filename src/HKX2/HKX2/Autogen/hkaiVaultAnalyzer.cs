using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiVaultAnalyzer : hkaiTraversalAnalyzer
    {
        public override uint Signature { get => 1223626616; }
        
        public float m_minWallWidth;
        public float m_maxWallWidth;
        public float m_minWallHeight;
        public float m_maxWallHeight;
        public float m_maxUpHeight;
        public float m_maxDownHeight;
        public float m_verticalApex;
        public float m_handPlantAngle;
        public float m_handPlantLeftExtent;
        public float m_handPlantRightExtent;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_minWallWidth = br.ReadSingle();
            m_maxWallWidth = br.ReadSingle();
            m_minWallHeight = br.ReadSingle();
            m_maxWallHeight = br.ReadSingle();
            m_maxUpHeight = br.ReadSingle();
            m_maxDownHeight = br.ReadSingle();
            m_verticalApex = br.ReadSingle();
            m_handPlantAngle = br.ReadSingle();
            m_handPlantLeftExtent = br.ReadSingle();
            m_handPlantRightExtent = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_minWallWidth);
            bw.WriteSingle(m_maxWallWidth);
            bw.WriteSingle(m_minWallHeight);
            bw.WriteSingle(m_maxWallHeight);
            bw.WriteSingle(m_maxUpHeight);
            bw.WriteSingle(m_maxDownHeight);
            bw.WriteSingle(m_verticalApex);
            bw.WriteSingle(m_handPlantAngle);
            bw.WriteSingle(m_handPlantLeftExtent);
            bw.WriteSingle(m_handPlantRightExtent);
        }
    }
}

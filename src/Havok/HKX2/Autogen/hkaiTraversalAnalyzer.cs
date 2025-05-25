using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiTraversalAnalyzer : hkReferencedObject
    {
        public override uint Signature { get => 2769283252; }
        
        public float m_maxPlanarAngle;
        public float m_maxRelativeSlopeAngle;
        public uint m_userdata;
        public float m_baseCost;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_maxPlanarAngle = br.ReadSingle();
            m_maxRelativeSlopeAngle = br.ReadSingle();
            m_userdata = br.ReadUInt32();
            m_baseCost = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_maxPlanarAngle);
            bw.WriteSingle(m_maxRelativeSlopeAngle);
            bw.WriteUInt32(m_userdata);
            bw.WriteSingle(m_baseCost);
        }
    }
}

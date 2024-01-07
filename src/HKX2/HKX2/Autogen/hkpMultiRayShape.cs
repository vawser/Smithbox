using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpMultiRayShape : hkpShape
    {
        public override uint Signature { get => 2612131923; }
        
        public List<hkpMultiRayShapeRay> m_rays;
        public float m_rayPenetrationDistance;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_rays = des.ReadClassArray<hkpMultiRayShapeRay>(br);
            m_rayPenetrationDistance = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkpMultiRayShapeRay>(bw, m_rays);
            bw.WriteSingle(m_rayPenetrationDistance);
            bw.WriteUInt32(0);
        }
    }
}

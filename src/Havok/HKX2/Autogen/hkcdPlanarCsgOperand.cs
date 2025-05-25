using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdPlanarCsgOperand : hkReferencedObject
    {
        public override uint Signature { get => 1073957566; }
        
        public hkcdPlanarGeometry m_geometry;
        public hkcdPlanarGeometry m_danglingGeometry;
        public hkcdPlanarSolid m_solid;
        public List<hkcdPlanarCsgOperandGeomSource> m_geomSources;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_geometry = des.ReadClassPointer<hkcdPlanarGeometry>(br);
            m_danglingGeometry = des.ReadClassPointer<hkcdPlanarGeometry>(br);
            m_solid = des.ReadClassPointer<hkcdPlanarSolid>(br);
            m_geomSources = des.ReadClassArray<hkcdPlanarCsgOperandGeomSource>(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkcdPlanarGeometry>(bw, m_geometry);
            s.WriteClassPointer<hkcdPlanarGeometry>(bw, m_danglingGeometry);
            s.WriteClassPointer<hkcdPlanarSolid>(bw, m_solid);
            s.WriteClassArray<hkcdPlanarCsgOperandGeomSource>(bw, m_geomSources);
            bw.WriteUInt64(0);
        }
    }
}

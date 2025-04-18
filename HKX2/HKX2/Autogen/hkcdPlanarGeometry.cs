using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdPlanarGeometry : hkcdPlanarEntity
    {
        public override uint Signature { get => 2937159141; }
        
        public hkcdPlanarGeometryPlanesCollection m_planes;
        public hkcdPlanarGeometryPolygonCollection m_polys;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_planes = des.ReadClassPointer<hkcdPlanarGeometryPlanesCollection>(br);
            m_polys = des.ReadClassPointer<hkcdPlanarGeometryPolygonCollection>(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkcdPlanarGeometryPlanesCollection>(bw, m_planes);
            s.WriteClassPointer<hkcdPlanarGeometryPolygonCollection>(bw, m_polys);
            bw.WriteUInt64(0);
        }
    }
}

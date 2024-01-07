using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiCuttingGeometryInfo : hkReferencedObject
    {
        public override uint Signature { get => 2451858849; }
        
        public hkGeometry m_geometry;
        public hkBitField m_cuttingTriangles;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_geometry = new hkGeometry();
            m_geometry.Read(des, br);
            m_cuttingTriangles = new hkBitField();
            m_cuttingTriangles.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_geometry.Write(s, bw);
            m_cuttingTriangles.Write(s, bw);
        }
    }
}

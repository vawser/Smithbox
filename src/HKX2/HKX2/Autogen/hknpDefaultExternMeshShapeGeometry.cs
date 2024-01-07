using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpDefaultExternMeshShapeGeometry : hknpExternMeshShapeGeometry
    {
        public override uint Signature { get => 4153572698; }
        
        public hkGeometry m_geometry;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_geometry = des.ReadClassPointer<hkGeometry>(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkGeometry>(bw, m_geometry);
            bw.WriteUInt64(0);
        }
    }
}

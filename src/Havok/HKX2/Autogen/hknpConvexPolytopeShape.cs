using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpConvexPolytopeShape : hknpConvexShape
    {
        public override uint Signature { get => 1021948899; }
        
        public List<Vector4> m_planes;
        public List<hknpConvexPolytopeShapeFace> m_faces;
        public List<byte> m_indices;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_planes = des.ReadVector4RelArray(br);
            m_faces = des.ReadClassRelArray<hknpConvexPolytopeShapeFace>(br);
            m_indices = des.ReadByteRelArray(br);
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            // Read TYPE_RELARRAY
            // Read TYPE_RELARRAY
            // Read TYPE_RELARRAY
            bw.WriteUInt32(0);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpCompressedMeshShapeInternalsKeyMask : hknpShapeKeyMask
    {
        public override uint Signature { get => 2313778874; }
        
        public hknpCompressedMeshShape m_shape;
        public List<uint> m_filter;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_shape = des.ReadClassPointer<hknpCompressedMeshShape>(br);
            m_filter = des.ReadUInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hknpCompressedMeshShape>(bw, m_shape);
            s.WriteUInt32Array(bw, m_filter);
        }
    }
}

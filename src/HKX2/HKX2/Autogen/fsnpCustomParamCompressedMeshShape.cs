using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class fsnpCustomParamCompressedMeshShape : hknpCompressedMeshShape
    {
        public override uint Signature { get => 3676312359; }
        
        public fsnpCustomMeshParameter m_pParam;
        public List<uint> m_triangleIndexToShapeKey;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_pParam = des.ReadClassPointer<fsnpCustomMeshParameter>(br);
            m_triangleIndexToShapeKey = des.ReadUInt32Array(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<fsnpCustomMeshParameter>(bw, m_pParam);
            s.WriteUInt32Array(bw, m_triangleIndexToShapeKey);
            bw.WriteUInt64(0);
        }
    }
}

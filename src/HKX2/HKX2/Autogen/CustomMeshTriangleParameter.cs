using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class CustomMeshTriangleParameter : CustomMeshParameter
    {
        public override uint Signature { get => 1677422640; }
        
        public List<byte> m_triangleDataBuffer;
        public int m_triangleDataStride;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            // Read TYPE_SIMPLEARRAY
            m_triangleDataStride = br.ReadInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            // Read TYPE_SIMPLEARRAY
            bw.WriteInt32(m_triangleDataStride);
        }
    }
}

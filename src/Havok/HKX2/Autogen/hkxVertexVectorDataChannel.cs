using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxVertexVectorDataChannel : hkReferencedObject
    {
        public override uint Signature { get => 2023230719; }
        
        public List<float> m_perVertexVectors;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_perVertexVectors = des.ReadSingleArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteSingleArray(bw, m_perVertexVectors);
        }
    }
}

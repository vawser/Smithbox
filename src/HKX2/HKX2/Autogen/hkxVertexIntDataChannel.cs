using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxVertexIntDataChannel : hkReferencedObject
    {
        public override uint Signature { get => 602688789; }
        
        public List<int> m_perVertexInts;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_perVertexInts = des.ReadInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteInt32Array(bw, m_perVertexInts);
        }
    }
}

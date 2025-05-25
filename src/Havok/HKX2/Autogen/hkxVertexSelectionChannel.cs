using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxVertexSelectionChannel : hkReferencedObject
    {
        public override uint Signature { get => 606589984; }
        
        public List<int> m_selectedVertices;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_selectedVertices = des.ReadInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteInt32Array(bw, m_selectedVertices);
        }
    }
}

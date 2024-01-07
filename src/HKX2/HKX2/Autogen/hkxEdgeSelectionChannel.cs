using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxEdgeSelectionChannel : hkReferencedObject
    {
        public override uint Signature { get => 3380368957; }
        
        public List<int> m_selectedEdges;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_selectedEdges = des.ReadInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteInt32Array(bw, m_selectedEdges);
        }
    }
}

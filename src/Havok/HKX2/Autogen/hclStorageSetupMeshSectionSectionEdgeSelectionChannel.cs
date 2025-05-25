using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStorageSetupMeshSectionSectionEdgeSelectionChannel : hkReferencedObject
    {
        public override uint Signature { get => 1016706522; }
        
        public List<uint> m_edgeIndices;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_edgeIndices = des.ReadUInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteUInt32Array(bw, m_edgeIndices);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStorageSetupMeshSectionSectionTriangleSelectionChannel : hkReferencedObject
    {
        public override uint Signature { get => 2258307042; }
        
        public List<uint> m_triangleIndices;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_triangleIndices = des.ReadUInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteUInt32Array(bw, m_triangleIndices);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpCompressedMeshShapeData : hkReferencedObject
    {
        public override uint Signature { get => 2730359897; }
        
        public hknpCompressedMeshShapeTree m_meshTree;
        public hkcdSimdTree m_simdTree;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_meshTree = new hknpCompressedMeshShapeTree();
            m_meshTree.Read(des, br);
            m_simdTree = new hkcdSimdTree();
            m_simdTree.Read(des, br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_meshTree.Write(s, bw);
            m_simdTree.Write(s, bw);
            bw.WriteUInt64(0);
        }
    }
}

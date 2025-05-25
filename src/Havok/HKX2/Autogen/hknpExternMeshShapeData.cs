using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpExternMeshShapeData : hkReferencedObject
    {
        public override uint Signature { get => 793512259; }
        
        public hkcdStaticTreeDefaultTreeStorage6 m_aabbTree;
        public hkcdSimdTree m_simdTree;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_aabbTree = new hkcdStaticTreeDefaultTreeStorage6();
            m_aabbTree.Read(des, br);
            m_simdTree = new hkcdSimdTree();
            m_simdTree.Read(des, br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_aabbTree.Write(s, bw);
            m_simdTree.Write(s, bw);
            bw.WriteUInt64(0);
        }
    }
}

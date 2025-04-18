using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpStaticCompoundShapeData : hkReferencedObject
    {
        public override uint Signature { get => 1339588569; }
        
        public hknpStaticCompoundShapeTree m_aabbTree;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_aabbTree = new hknpStaticCompoundShapeTree();
            m_aabbTree.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_aabbTree.Write(s, bw);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpDynamicCompoundShapeData : hkReferencedObject
    {
        public override uint Signature { get => 4080911308; }
        
        public hknpDynamicCompoundShapeTree m_aabbTree;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_aabbTree = new hknpDynamicCompoundShapeTree();
            m_aabbTree.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_aabbTree.Write(s, bw);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpBreakableShape : hkReferencedObject
    {
        public override uint Signature { get => 145098340; }
        
        public hkcdShape m_physicsShape;
        public hkpBreakableMaterial m_material;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_physicsShape = des.ReadClassPointer<hkcdShape>(br);
            m_material = des.ReadClassPointer<hkpBreakableMaterial>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkcdShape>(bw, m_physicsShape);
            s.WriteClassPointer<hkpBreakableMaterial>(bw, m_material);
        }
    }
}

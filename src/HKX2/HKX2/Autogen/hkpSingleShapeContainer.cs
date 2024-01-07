using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpSingleShapeContainer : hkpShapeContainer
    {
        public override uint Signature { get => 1940528440; }
        
        public hkpShape m_childShape;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_childShape = des.ReadClassPointer<hkpShape>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkpShape>(bw, m_childShape);
        }
    }
}

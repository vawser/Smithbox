using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpConvexTransformShapeBase : hkpConvexShape
    {
        public override uint Signature { get => 1778796008; }
        
        public hkpSingleShapeContainer m_childShape;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_childShape = new hkpSingleShapeContainer();
            m_childShape.Read(des, br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_childShape.Write(s, bw);
            bw.WriteUInt64(0);
        }
    }
}

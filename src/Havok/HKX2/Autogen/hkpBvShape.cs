using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpBvShape : hkpShape
    {
        public override uint Signature { get => 2915716736; }
        
        public hkpShape m_boundingVolumeShape;
        public hkpSingleShapeContainer m_childShape;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_boundingVolumeShape = des.ReadClassPointer<hkpShape>(br);
            m_childShape = new hkpSingleShapeContainer();
            m_childShape.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkpShape>(bw, m_boundingVolumeShape);
            m_childShape.Write(s, bw);
        }
    }
}

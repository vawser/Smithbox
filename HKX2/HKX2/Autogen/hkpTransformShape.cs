using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpTransformShape : hkpShape
    {
        public override uint Signature { get => 783161109; }
        
        public hkpSingleShapeContainer m_childShape;
        public Quaternion m_rotation;
        public Matrix4x4 m_transform;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_childShape = new hkpSingleShapeContainer();
            m_childShape.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            m_rotation = des.ReadQuaternion(br);
            m_transform = des.ReadTransform(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_childShape.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            s.WriteQuaternion(bw, m_rotation);
            s.WriteTransform(bw, m_transform);
        }
    }
}

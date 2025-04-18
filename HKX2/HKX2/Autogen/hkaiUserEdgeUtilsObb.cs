using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiUserEdgeUtilsObb : IHavokObject
    {
        public virtual uint Signature { get => 1221661850; }
        
        public Matrix4x4 m_transform;
        public Vector4 m_halfExtents;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transform = des.ReadTransform(br);
            m_halfExtents = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteTransform(bw, m_transform);
            s.WriteVector4(bw, m_halfExtents);
        }
    }
}

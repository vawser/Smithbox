using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiReferenceFrame : IHavokObject
    {
        public virtual uint Signature { get => 1663224485; }
        
        public Matrix4x4 m_transform;
        public Vector4 m_linearVelocity;
        public Vector4 m_angularVelocity;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transform = des.ReadTransform(br);
            m_linearVelocity = des.ReadVector4(br);
            m_angularVelocity = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteTransform(bw, m_transform);
            s.WriteVector4(bw, m_linearVelocity);
            s.WriteVector4(bw, m_angularVelocity);
        }
    }
}

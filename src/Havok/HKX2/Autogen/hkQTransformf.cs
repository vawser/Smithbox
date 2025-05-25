using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkQTransformf : IHavokObject
    {
        public virtual uint Signature { get => 1192894958; }
        
        public Quaternion m_rotation;
        public Vector4 m_translation;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_rotation = des.ReadQuaternion(br);
            m_translation = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteQuaternion(bw, m_rotation);
            s.WriteVector4(bw, m_translation);
        }
    }
}

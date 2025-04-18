using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiSilhouetteReferenceFrame : IHavokObject
    {
        public virtual uint Signature { get => 1006741638; }
        
        public Vector4 m_up;
        public Vector4 m_referenceAxis;
        public Vector4 m_orthogonalAxis;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_up = des.ReadVector4(br);
            m_referenceAxis = des.ReadVector4(br);
            m_orthogonalAxis = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_up);
            s.WriteVector4(bw, m_referenceAxis);
            s.WriteVector4(bw, m_orthogonalAxis);
        }
    }
}

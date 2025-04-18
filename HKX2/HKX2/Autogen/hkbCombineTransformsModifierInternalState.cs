using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCombineTransformsModifierInternalState : hkReferencedObject
    {
        public override uint Signature { get => 2692817096; }
        
        public Vector4 m_translationOut;
        public Quaternion m_rotationOut;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_translationOut = des.ReadVector4(br);
            m_rotationOut = des.ReadQuaternion(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_translationOut);
            s.WriteQuaternion(bw, m_rotationOut);
        }
    }
}

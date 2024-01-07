using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbTransformVectorModifierInternalState : hkReferencedObject
    {
        public override uint Signature { get => 2936483583; }
        
        public Vector4 m_vectorOut;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_vectorOut = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_vectorOut);
        }
    }
}

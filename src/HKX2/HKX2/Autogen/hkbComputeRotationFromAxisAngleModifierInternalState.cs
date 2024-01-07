using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbComputeRotationFromAxisAngleModifierInternalState : hkReferencedObject
    {
        public override uint Signature { get => 1265518636; }
        
        public Quaternion m_rotationOut;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_rotationOut = des.ReadQuaternion(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteQuaternion(bw, m_rotationOut);
        }
    }
}

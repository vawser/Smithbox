using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRotateCharacterModifierInternalState : hkReferencedObject
    {
        public override uint Signature { get => 147123319; }
        
        public float m_angle;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_angle = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_angle);
            bw.WriteUInt32(0);
        }
    }
}

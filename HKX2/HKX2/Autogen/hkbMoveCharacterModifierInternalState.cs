using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbMoveCharacterModifierInternalState : hkReferencedObject
    {
        public override uint Signature { get => 2254275996; }
        
        public float m_timeSinceLastModify;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_timeSinceLastModify = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_timeSinceLastModify);
            bw.WriteUInt32(0);
        }
    }
}

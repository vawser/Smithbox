using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbTimerModifierInternalState : hkReferencedObject
    {
        public override uint Signature { get => 1731128423; }
        
        public float m_secondsElapsed;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_secondsElapsed = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_secondsElapsed);
            bw.WriteUInt32(0);
        }
    }
}

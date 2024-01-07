using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbDelayedModifier : hkbModifierWrapper
    {
        public override uint Signature { get => 4100756999; }
        
        public float m_delaySeconds;
        public float m_durationSeconds;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_delaySeconds = br.ReadSingle();
            m_durationSeconds = br.ReadSingle();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_delaySeconds);
            bw.WriteSingle(m_durationSeconds);
            bw.WriteUInt64(0);
        }
    }
}

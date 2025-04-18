using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbTimerModifier : hkbModifier
    {
        public override uint Signature { get => 2021705186; }
        
        public float m_alarmTimeSeconds;
        public hkbEventProperty m_alarmEvent;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_alarmTimeSeconds = br.ReadSingle();
            br.ReadUInt32();
            m_alarmEvent = new hkbEventProperty();
            m_alarmEvent.Read(des, br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_alarmTimeSeconds);
            bw.WriteUInt32(0);
            m_alarmEvent.Write(s, bw);
            bw.WriteUInt64(0);
        }
    }
}

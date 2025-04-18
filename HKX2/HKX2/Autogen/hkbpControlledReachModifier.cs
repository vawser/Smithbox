using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbpControlledReachModifier : hkbModifier
    {
        public override uint Signature { get => 1867942635; }
        
        public float m_fadeInStart;
        public float m_fadeInEnd;
        public float m_fadeOutStart;
        public float m_fadeOutEnd;
        public float m_fadeOutDuration;
        public float m_sensorAngle;
        public short m_handIndex_0;
        public short m_handIndex_1;
        public bool m_isHandEnabled_0;
        public bool m_isHandEnabled_1;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt32();
            m_fadeInStart = br.ReadSingle();
            m_fadeInEnd = br.ReadSingle();
            m_fadeOutStart = br.ReadSingle();
            m_fadeOutEnd = br.ReadSingle();
            m_fadeOutDuration = br.ReadSingle();
            m_sensorAngle = br.ReadSingle();
            m_handIndex_0 = br.ReadInt16();
            m_handIndex_1 = br.ReadInt16();
            m_isHandEnabled_0 = br.ReadBoolean();
            m_isHandEnabled_1 = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteSingle(m_fadeInStart);
            bw.WriteSingle(m_fadeInEnd);
            bw.WriteSingle(m_fadeOutStart);
            bw.WriteSingle(m_fadeOutEnd);
            bw.WriteSingle(m_fadeOutDuration);
            bw.WriteSingle(m_sensorAngle);
            bw.WriteInt16(m_handIndex_0);
            bw.WriteInt16(m_handIndex_1);
            bw.WriteBoolean(m_isHandEnabled_0);
            bw.WriteBoolean(m_isHandEnabled_1);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}

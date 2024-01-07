using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbPoseMatchingGeneratorInternalState : hkReferencedObject
    {
        public override uint Signature { get => 396665285; }
        
        public int m_currentMatch;
        public int m_bestMatch;
        public float m_timeSinceBetterMatch;
        public float m_error;
        public bool m_resetCurrentMatchLocalTime;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_currentMatch = br.ReadInt32();
            m_bestMatch = br.ReadInt32();
            m_timeSinceBetterMatch = br.ReadSingle();
            m_error = br.ReadSingle();
            m_resetCurrentMatchLocalTime = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt32(m_currentMatch);
            bw.WriteInt32(m_bestMatch);
            bw.WriteSingle(m_timeSinceBetterMatch);
            bw.WriteSingle(m_error);
            bw.WriteBoolean(m_resetCurrentMatchLocalTime);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}

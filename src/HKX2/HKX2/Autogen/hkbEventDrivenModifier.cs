using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbEventDrivenModifier : hkbModifierWrapper
    {
        public override uint Signature { get => 904869227; }
        
        public int m_activateEventId;
        public int m_deactivateEventId;
        public bool m_activeByDefault;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_activateEventId = br.ReadInt32();
            m_deactivateEventId = br.ReadInt32();
            m_activeByDefault = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt32(m_activateEventId);
            bw.WriteInt32(m_deactivateEventId);
            bw.WriteBoolean(m_activeByDefault);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum Event
    {
        REMOVED_FROM_WORLD = 0,
        SHOWN = 1,
        HIDDEN = 2,
        ACTIVATED = 3,
        DEACTIVATED = 4,
    }
    
    public partial class hkbCharacterInfo : hkReferencedObject
    {
        public override uint Signature { get => 3856911078; }
        
        public ulong m_characterId;
        public Event m_event;
        public int m_padding;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_characterId = br.ReadUInt64();
            m_event = (Event)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_padding = br.ReadInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_characterId);
            bw.WriteByte((byte)m_event);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_padding);
        }
    }
}

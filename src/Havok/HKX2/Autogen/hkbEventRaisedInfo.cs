using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbEventRaisedInfo : hkReferencedObject
    {
        public override uint Signature { get => 3019399239; }
        
        public ulong m_characterId;
        public string m_eventName;
        public bool m_raisedBySdk;
        public int m_senderId;
        public int m_padding;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_characterId = br.ReadUInt64();
            m_eventName = des.ReadStringPointer(br);
            m_raisedBySdk = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_senderId = br.ReadInt32();
            m_padding = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_characterId);
            s.WriteStringPointer(bw, m_eventName);
            bw.WriteBoolean(m_raisedBySdk);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_senderId);
            bw.WriteInt32(m_padding);
            bw.WriteUInt32(0);
        }
    }
}

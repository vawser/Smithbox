using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbBehaviorEventsInfo : hkReferencedObject
    {
        public override uint Signature { get => 1694681928; }
        
        public ulong m_characterId;
        public List<short> m_externalEventIds;
        public int m_padding;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_characterId = br.ReadUInt64();
            m_externalEventIds = des.ReadInt16Array(br);
            m_padding = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_characterId);
            s.WriteInt16Array(bw, m_externalEventIds);
            bw.WriteInt32(m_padding);
            bw.WriteUInt32(0);
        }
    }
}

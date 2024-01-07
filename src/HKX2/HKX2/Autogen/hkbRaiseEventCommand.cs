using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRaiseEventCommand : hkReferencedObject
    {
        public override uint Signature { get => 3354722925; }
        
        public ulong m_characterId;
        public bool m_global;
        public int m_externalId;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_characterId = br.ReadUInt64();
            m_global = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_externalId = br.ReadInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_characterId);
            bw.WriteBoolean(m_global);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_externalId);
        }
    }
}

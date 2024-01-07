using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCharacterSelectedInfo : hkReferencedObject
    {
        public override uint Signature { get => 4123522249; }
        
        public ulong m_characterId;
        public int m_scriptDebuggingPort;
        public int m_padding;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_characterId = br.ReadUInt64();
            m_scriptDebuggingPort = br.ReadInt32();
            m_padding = br.ReadInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_characterId);
            bw.WriteInt32(m_scriptDebuggingPort);
            bw.WriteInt32(m_padding);
        }
    }
}

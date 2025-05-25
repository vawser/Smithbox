using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum CharacterControlCommand
    {
        COMMAND_HIDE = 0,
        COMMAND_SHOW = 1,
    }
    
    public partial class hkbCharacterControlCommand : hkReferencedObject
    {
        public override uint Signature { get => 3263284257; }
        
        public ulong m_characterId;
        public CharacterControlCommand m_command;
        public int m_padding;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_characterId = br.ReadUInt64();
            m_command = (CharacterControlCommand)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_padding = br.ReadInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_characterId);
            bw.WriteByte((byte)m_command);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_padding);
        }
    }
}

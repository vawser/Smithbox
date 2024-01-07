using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbSetWordVariableCommand : hkReferencedObject
    {
        public override uint Signature { get => 4274729204; }
        
        public ulong m_characterId;
        public string m_variableName;
        public hkbVariableValue m_value;
        public Vector4 m_quadValue;
        public VariableType m_type;
        public bool m_global;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_characterId = br.ReadUInt64();
            m_variableName = des.ReadStringPointer(br);
            m_value = new hkbVariableValue();
            m_value.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt32();
            m_quadValue = des.ReadVector4(br);
            m_type = (VariableType)br.ReadByte();
            m_global = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_characterId);
            s.WriteStringPointer(bw, m_variableName);
            m_value.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            s.WriteVector4(bw, m_quadValue);
            bw.WriteByte((byte)m_type);
            bw.WriteBoolean(m_global);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkVariableTweakingHelperIntVariableInfo : IHavokObject
    {
        public virtual uint Signature { get => 3680626727; }
        
        public string m_name;
        public int m_value;
        public bool m_tweakOn;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            m_value = br.ReadInt32();
            m_tweakOn = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            bw.WriteInt32(m_value);
            bw.WriteBoolean(m_tweakOn);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}

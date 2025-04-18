using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkVariableTweakingHelperBoolVariableInfo : IHavokObject
    {
        public virtual uint Signature { get => 1731769824; }
        
        public string m_name;
        public bool m_value;
        public bool m_tweakOn;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            m_value = br.ReadBoolean();
            m_tweakOn = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            bw.WriteBoolean(m_value);
            bw.WriteBoolean(m_tweakOn);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}

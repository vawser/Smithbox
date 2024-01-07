using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkVariableTweakingHelperVector4VariableInfo : IHavokObject
    {
        public virtual uint Signature { get => 1063280754; }
        
        public string m_name;
        public float m_x;
        public float m_y;
        public float m_z;
        public float m_w;
        public bool m_tweakOn;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            m_x = br.ReadSingle();
            m_y = br.ReadSingle();
            m_z = br.ReadSingle();
            m_w = br.ReadSingle();
            m_tweakOn = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            bw.WriteSingle(m_x);
            bw.WriteSingle(m_y);
            bw.WriteSingle(m_z);
            bw.WriteSingle(m_w);
            bw.WriteBoolean(m_tweakOn);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}

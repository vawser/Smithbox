using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbBoolVariableSequencedDataSample : IHavokObject
    {
        public virtual uint Signature { get => 1363633116; }
        
        public float m_time;
        public bool m_value;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_time = br.ReadSingle();
            m_value = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_time);
            bw.WriteBoolean(m_value);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}

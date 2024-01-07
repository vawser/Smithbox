using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRealVariableSequencedDataSample : IHavokObject
    {
        public virtual uint Signature { get => 3144715197; }
        
        public float m_time;
        public float m_value;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_time = br.ReadSingle();
            m_value = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_time);
            bw.WriteSingle(m_value);
        }
    }
}

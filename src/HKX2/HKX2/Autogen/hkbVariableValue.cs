using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbVariableValue : IHavokObject
    {
        public virtual uint Signature { get => 194624874; }
        
        public int m_value;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_value = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_value);
        }
    }
}

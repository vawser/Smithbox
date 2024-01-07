using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkIntRealPair : IHavokObject
    {
        public virtual uint Signature { get => 1196465826; }
        
        public int m_key;
        public float m_value;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_key = br.ReadInt32();
            m_value = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_key);
            bw.WriteSingle(m_value);
        }
    }
}

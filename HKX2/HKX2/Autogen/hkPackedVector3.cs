using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkPackedVector3 : IHavokObject
    {
        public virtual uint Signature { get => 2191969155; }
        
        public short m_values_0;
        public short m_values_1;
        public short m_values_2;
        public short m_values_3;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_values_0 = br.ReadInt16();
            m_values_1 = br.ReadInt16();
            m_values_2 = br.ReadInt16();
            m_values_3 = br.ReadInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt16(m_values_0);
            bw.WriteInt16(m_values_1);
            bw.WriteInt16(m_values_2);
            bw.WriteInt16(m_values_3);
        }
    }
}

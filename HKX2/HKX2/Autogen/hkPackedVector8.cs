using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkPackedVector8 : IHavokObject
    {
        public virtual uint Signature { get => 1932229307; }
        
        public sbyte m_values_0;
        public sbyte m_values_1;
        public sbyte m_values_2;
        public sbyte m_values_3;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_values_0 = br.ReadSByte();
            m_values_1 = br.ReadSByte();
            m_values_2 = br.ReadSByte();
            m_values_3 = br.ReadSByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSByte(m_values_0);
            bw.WriteSByte(m_values_1);
            bw.WriteSByte(m_values_2);
            bw.WriteSByte(m_values_3);
        }
    }
}

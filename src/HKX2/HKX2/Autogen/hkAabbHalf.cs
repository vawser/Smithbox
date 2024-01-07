using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkAabbHalf : IHavokObject
    {
        public virtual uint Signature { get => 297169822; }
        
        public ushort m_data_0;
        public ushort m_data_1;
        public ushort m_data_2;
        public ushort m_data_3;
        public ushort m_data_4;
        public ushort m_data_5;
        public ushort m_data_6;
        public ushort m_data_7;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_data_0 = br.ReadUInt16();
            m_data_1 = br.ReadUInt16();
            m_data_2 = br.ReadUInt16();
            m_data_3 = br.ReadUInt16();
            m_data_4 = br.ReadUInt16();
            m_data_5 = br.ReadUInt16();
            m_data_6 = br.ReadUInt16();
            m_data_7 = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_data_0);
            bw.WriteUInt16(m_data_1);
            bw.WriteUInt16(m_data_2);
            bw.WriteUInt16(m_data_3);
            bw.WriteUInt16(m_data_4);
            bw.WriteUInt16(m_data_5);
            bw.WriteUInt16(m_data_6);
            bw.WriteUInt16(m_data_7);
        }
    }
}

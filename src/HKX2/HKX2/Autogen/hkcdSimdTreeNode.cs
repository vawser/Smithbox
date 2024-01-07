using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdSimdTreeNode : hkcdFourAabb
    {
        public override uint Signature { get => 3303278279; }
        
        public enum Flags
        {
            HAS_INTERNALS = 1,
            HAS_LEAVES = 2,
            HAS_NULLS = 4,
        }
        
        public uint m_data_0;
        public uint m_data_1;
        public uint m_data_2;
        public uint m_data_3;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_data_0 = br.ReadUInt32();
            m_data_1 = br.ReadUInt32();
            m_data_2 = br.ReadUInt32();
            m_data_3 = br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt32(m_data_0);
            bw.WriteUInt32(m_data_1);
            bw.WriteUInt32(m_data_2);
            bw.WriteUInt32(m_data_3);
        }
    }
}

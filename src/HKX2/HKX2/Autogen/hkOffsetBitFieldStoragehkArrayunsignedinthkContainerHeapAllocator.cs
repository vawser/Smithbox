using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkOffsetBitFieldStoragehkArrayunsignedinthkContainerHeapAllocator : IHavokObject
    {
        public virtual uint Signature { get => 662584337; }
        
        public List<uint> m_words;
        public int m_offset;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_words = des.ReadUInt32Array(br);
            m_offset = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteUInt32Array(bw, m_words);
            bw.WriteInt32(m_offset);
            bw.WriteUInt32(0);
        }
    }
}

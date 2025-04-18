using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiDirectedGraphInstanceFreeBlockList : IHavokObject
    {
        public virtual uint Signature { get => 846755607; }
        
        public List<int> m_blocks;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_blocks = des.ReadInt32Array(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteInt32Array(bw, m_blocks);
        }
    }
}

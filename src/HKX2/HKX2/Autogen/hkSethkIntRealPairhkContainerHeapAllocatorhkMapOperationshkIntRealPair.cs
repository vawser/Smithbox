using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkSethkIntRealPairhkContainerHeapAllocatorhkMapOperationshkIntRealPair : IHavokObject
    {
        public virtual uint Signature { get => 264794137; }
        
        public List<hkIntRealPair> m_elem;
        public int m_numElems;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_elem = des.ReadClassArray<hkIntRealPair>(br);
            m_numElems = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hkIntRealPair>(bw, m_elem);
            bw.WriteInt32(m_numElems);
            bw.WriteUInt32(0);
        }
    }
}

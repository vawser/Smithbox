using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkFreeListArrayhknpMotionPropertieshknpMotionPropertiesId8hknpMotionPropertiesFreeListArrayOperations : IHavokObject
    {
        public virtual uint Signature { get => 2567173201; }
        
        public List<hknpMotionProperties> m_elements;
        public int m_firstFree;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_elements = des.ReadClassArray<hknpMotionProperties>(br);
            m_firstFree = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hknpMotionProperties>(bw, m_elements);
            bw.WriteInt32(m_firstFree);
            bw.WriteUInt32(0);
        }
    }
}

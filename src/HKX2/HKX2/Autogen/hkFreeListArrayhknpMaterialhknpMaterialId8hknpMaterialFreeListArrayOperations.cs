using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkFreeListArrayhknpMaterialhknpMaterialId8hknpMaterialFreeListArrayOperations : IHavokObject
    {
        public virtual uint Signature { get => 2053630430; }
        
        public List<hknpMaterial> m_elements;
        public int m_firstFree;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_elements = des.ReadClassArray<hknpMaterial>(br);
            m_firstFree = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hknpMaterial>(bw, m_elements);
            bw.WriteInt32(m_firstFree);
            bw.WriteUInt32(0);
        }
    }
}

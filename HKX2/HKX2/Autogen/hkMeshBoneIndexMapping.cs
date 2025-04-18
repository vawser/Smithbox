using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMeshBoneIndexMapping : IHavokObject
    {
        public virtual uint Signature { get => 1219292021; }
        
        public List<short> m_mapping;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_mapping = des.ReadInt16Array(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteInt16Array(bw, m_mapping);
        }
    }
}

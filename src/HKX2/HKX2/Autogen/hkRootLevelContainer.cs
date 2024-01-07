using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkRootLevelContainer : IHavokObject
    {
        public virtual uint Signature { get => 661831966; }
        
        public List<hkRootLevelContainerNamedVariant> m_namedVariants;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_namedVariants = des.ReadClassArray<hkRootLevelContainerNamedVariant>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hkRootLevelContainerNamedVariant>(bw, m_namedVariants);
        }
    }
}

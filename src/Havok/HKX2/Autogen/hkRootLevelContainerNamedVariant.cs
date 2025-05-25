using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkRootLevelContainerNamedVariant : IHavokObject
    {
        public virtual uint Signature { get => 2969805517; }
        
        public string m_name;
        public string m_className;
        public hkReferencedObject m_variant;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            m_className = des.ReadStringPointer(br);
            m_variant = des.ReadClassPointer<hkReferencedObject>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            s.WriteStringPointer(bw, m_className);
            s.WriteClassPointer<hkReferencedObject>(bw, m_variant);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkCustomAttributes : IHavokObject
    {
        public virtual uint Signature { get => 3220279301; }
        
        public List<hkCustomAttributesAttribute> m_attributes;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            // Read TYPE_SIMPLEARRAY
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            // Read TYPE_SIMPLEARRAY
        }
    }
}

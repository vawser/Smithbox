using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum Hint
    {
        HINT_NONE = 0,
        HINT_IGNORE = 1,
        HINT_TRANSFORM = 2,
        HINT_SCALE = 4,
        HINT_TRANSFORM_AND_SCALE = 6,
        HINT_FLIP = 8,
    }
    
    public partial class hkxAttribute : IHavokObject
    {
        public virtual uint Signature { get => 1937099491; }
        
        public string m_name;
        public hkReferencedObject m_value;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            m_value = des.ReadClassPointer<hkReferencedObject>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hkReferencedObject>(bw, m_value);
        }
    }
}

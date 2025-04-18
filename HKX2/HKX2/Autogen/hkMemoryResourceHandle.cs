using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMemoryResourceHandle : hkResourceHandle
    {
        public override uint Signature { get => 3327040450; }
        
        public hkReferencedObject m_variant;
        public string m_name;
        public List<hkMemoryResourceHandleExternalLink> m_references;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_variant = des.ReadClassPointer<hkReferencedObject>(br);
            m_name = des.ReadStringPointer(br);
            m_references = des.ReadClassArray<hkMemoryResourceHandleExternalLink>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkReferencedObject>(bw, m_variant);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassArray<hkMemoryResourceHandleExternalLink>(bw, m_references);
        }
    }
}

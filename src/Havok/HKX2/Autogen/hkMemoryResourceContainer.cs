using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMemoryResourceContainer : hkResourceContainer
    {
        public override uint Signature { get => 501299827; }
        
        public string m_name;
        public List<hkMemoryResourceHandle> m_resourceHandles;
        public List<hkMemoryResourceContainer> m_children;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            br.ReadUInt64();
            m_resourceHandles = des.ReadClassPointerArray<hkMemoryResourceHandle>(br);
            m_children = des.ReadClassPointerArray<hkMemoryResourceContainer>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            bw.WriteUInt64(0);
            s.WriteClassPointerArray<hkMemoryResourceHandle>(bw, m_resourceHandles);
            s.WriteClassPointerArray<hkMemoryResourceContainer>(bw, m_children);
        }
    }
}

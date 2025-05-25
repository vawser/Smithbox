using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticTreeDynamicStoragehkcdStaticTreeCodecRaw : IHavokObject
    {
        public virtual uint Signature { get => 2231710934; }
        
        public List<hkcdStaticTreeCodecRaw> m_nodes;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_nodes = des.ReadClassArray<hkcdStaticTreeCodecRaw>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hkcdStaticTreeCodecRaw>(bw, m_nodes);
        }
    }
}

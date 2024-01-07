using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticTreeDynamicStoragehkcdStaticTreeCodec3Axis4 : IHavokObject
    {
        public virtual uint Signature { get => 667684532; }
        
        public List<hkcdStaticTreeCodec3Axis4> m_nodes;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_nodes = des.ReadClassArray<hkcdStaticTreeCodec3Axis4>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hkcdStaticTreeCodec3Axis4>(bw, m_nodes);
        }
    }
}

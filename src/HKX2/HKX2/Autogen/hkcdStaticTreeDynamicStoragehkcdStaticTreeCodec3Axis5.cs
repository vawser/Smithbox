using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticTreeDynamicStoragehkcdStaticTreeCodec3Axis5 : IHavokObject
    {
        public virtual uint Signature { get => 153534671; }
        
        public List<hkcdStaticTreeCodec3Axis5> m_nodes;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_nodes = des.ReadClassArray<hkcdStaticTreeCodec3Axis5>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hkcdStaticTreeCodec3Axis5>(bw, m_nodes);
        }
    }
}

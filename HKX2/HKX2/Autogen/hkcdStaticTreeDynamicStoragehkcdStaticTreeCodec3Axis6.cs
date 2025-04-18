using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticTreeDynamicStoragehkcdStaticTreeCodec3Axis6 : IHavokObject
    {
        public virtual uint Signature { get => 3669965013; }
        
        public List<hkcdStaticTreeCodec3Axis6> m_nodes;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_nodes = des.ReadClassArray<hkcdStaticTreeCodec3Axis6>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hkcdStaticTreeCodec3Axis6>(bw, m_nodes);
        }
    }
}

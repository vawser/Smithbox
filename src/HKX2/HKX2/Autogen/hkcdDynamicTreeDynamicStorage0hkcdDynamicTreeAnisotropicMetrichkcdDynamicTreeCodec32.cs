using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdDynamicTreeDynamicStorage0hkcdDynamicTreeAnisotropicMetrichkcdDynamicTreeCodec32 : hkcdDynamicTreeAnisotropicMetric
    {
        public override uint Signature { get => 238610403; }
        
        public List<hkcdDynamicTreeCodec32> m_nodes;
        public ushort m_firstFree;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_nodes = des.ReadClassArray<hkcdDynamicTreeCodec32>(br);
            m_firstFree = br.ReadUInt16();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkcdDynamicTreeCodec32>(bw, m_nodes);
            bw.WriteUInt16(m_firstFree);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}

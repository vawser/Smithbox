using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdDynamicTreeDynamicStorage0hkcdDynamicTreeAnisotropicMetrichkcdDynamicTreeCodecRawUlong : hkcdDynamicTreeAnisotropicMetric
    {
        public override uint Signature { get => 3965307088; }
        
        public List<hkcdDynamicTreeCodecRawUlong> m_nodes;
        public ulong m_firstFree;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_nodes = des.ReadClassArray<hkcdDynamicTreeCodecRawUlong>(br);
            m_firstFree = br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkcdDynamicTreeCodecRawUlong>(bw, m_nodes);
            bw.WriteUInt64(m_firstFree);
        }
    }
}

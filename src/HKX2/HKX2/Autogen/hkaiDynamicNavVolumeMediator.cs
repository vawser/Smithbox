using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiDynamicNavVolumeMediator : hkaiNavVolumeMediator
    {
        public override uint Signature { get => 578524699; }
        
        public hkaiStreamingCollection m_collection;
        public hkcdDynamicAabbTree m_aabbTree;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_collection = des.ReadClassPointer<hkaiStreamingCollection>(br);
            m_aabbTree = des.ReadClassPointer<hkcdDynamicAabbTree>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkaiStreamingCollection>(bw, m_collection);
            s.WriteClassPointer<hkcdDynamicAabbTree>(bw, m_aabbTree);
        }
    }
}

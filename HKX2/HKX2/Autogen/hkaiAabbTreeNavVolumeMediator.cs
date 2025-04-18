using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiAabbTreeNavVolumeMediator : hkaiNavVolumeMediator
    {
        public override uint Signature { get => 2902232628; }
        
        public hkaiNavVolume m_navVolume;
        public hkcdStaticAabbTree m_tree;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_navVolume = des.ReadClassPointer<hkaiNavVolume>(br);
            m_tree = des.ReadClassPointer<hkcdStaticAabbTree>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkaiNavVolume>(bw, m_navVolume);
            s.WriteClassPointer<hkcdStaticAabbTree>(bw, m_tree);
        }
    }
}

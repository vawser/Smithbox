using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiSilhouetteRecorderVolumeLoadedEvent : hkaiSilhouetteRecorderReplayEvent
    {
        public override uint Signature { get => 1732499054; }
        
        public hkaiNavVolumeInstance m_volume;
        public hkaiNavVolumeMediator m_mediator;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_volume = des.ReadClassPointer<hkaiNavVolumeInstance>(br);
            m_mediator = des.ReadClassPointer<hkaiNavVolumeMediator>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkaiNavVolumeInstance>(bw, m_volume);
            s.WriteClassPointer<hkaiNavVolumeMediator>(bw, m_mediator);
        }
    }
}

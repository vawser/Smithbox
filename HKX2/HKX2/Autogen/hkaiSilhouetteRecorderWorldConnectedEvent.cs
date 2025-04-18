using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiSilhouetteRecorderWorldConnectedEvent : hkaiSilhouetteRecorderReplayEvent
    {
        public override uint Signature { get => 2409019201; }
        
        public hkaiWorld m_world;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_world = des.ReadClassPointer<hkaiWorld>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkaiWorld>(bw, m_world);
        }
    }
}

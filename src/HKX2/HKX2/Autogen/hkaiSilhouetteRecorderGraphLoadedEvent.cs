using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiSilhouetteRecorderGraphLoadedEvent : hkaiSilhouetteRecorderReplayEvent
    {
        public override uint Signature { get => 3069279907; }
        
        public hkaiDirectedGraphInstance m_graph;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_graph = des.ReadClassPointer<hkaiDirectedGraphInstance>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkaiDirectedGraphInstance>(bw, m_graph);
        }
    }
}

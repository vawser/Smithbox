using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiSilhouetteRecorderInstanceLoadedEvent : hkaiSilhouetteRecorderReplayEvent
    {
        public override uint Signature { get => 2696523890; }
        
        public hkaiNavMeshInstance m_instance;
        public hkaiNavMeshQueryMediator m_mediator;
        public hkaiDirectedGraphInstance m_graph;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_instance = des.ReadClassPointer<hkaiNavMeshInstance>(br);
            m_mediator = des.ReadClassPointer<hkaiNavMeshQueryMediator>(br);
            m_graph = des.ReadClassPointer<hkaiDirectedGraphInstance>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkaiNavMeshInstance>(bw, m_instance);
            s.WriteClassPointer<hkaiNavMeshQueryMediator>(bw, m_mediator);
            s.WriteClassPointer<hkaiDirectedGraphInstance>(bw, m_graph);
        }
    }
}

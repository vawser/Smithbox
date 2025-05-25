using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiStreamingCollectionInstanceInfo : IHavokObject
    {
        public virtual uint Signature { get => 707033962; }
        
        public hkaiNavMeshInstance m_instancePtr;
        public hkaiNavVolumeInstance m_volumeInstancePtr;
        public hkaiDirectedGraphInstance m_clusterGraphInstance;
        public hkaiNavMeshQueryMediator m_mediator;
        public hkaiNavVolumeMediator m_volumeMediator;
        public uint m_treeNode;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_instancePtr = des.ReadClassPointer<hkaiNavMeshInstance>(br);
            m_volumeInstancePtr = des.ReadClassPointer<hkaiNavVolumeInstance>(br);
            m_clusterGraphInstance = des.ReadClassPointer<hkaiDirectedGraphInstance>(br);
            m_mediator = des.ReadClassPointer<hkaiNavMeshQueryMediator>(br);
            m_volumeMediator = des.ReadClassPointer<hkaiNavVolumeMediator>(br);
            m_treeNode = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkaiNavMeshInstance>(bw, m_instancePtr);
            s.WriteClassPointer<hkaiNavVolumeInstance>(bw, m_volumeInstancePtr);
            s.WriteClassPointer<hkaiDirectedGraphInstance>(bw, m_clusterGraphInstance);
            s.WriteClassPointer<hkaiNavMeshQueryMediator>(bw, m_mediator);
            s.WriteClassPointer<hkaiNavVolumeMediator>(bw, m_volumeMediator);
            bw.WriteUInt32(m_treeNode);
            bw.WriteUInt32(0);
        }
    }
}

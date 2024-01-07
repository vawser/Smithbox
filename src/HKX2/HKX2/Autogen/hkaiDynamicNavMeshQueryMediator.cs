using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiDynamicNavMeshQueryMediator : hkaiNavMeshQueryMediator
    {
        public override uint Signature { get => 1760073921; }
        
        public hkaiStreamingCollection m_collection;
        public hkcdDynamicAabbTree m_aabbTree;
        public hkaiNavMeshCutter m_cutter;
        public float m_cutAabbTolerance;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_collection = des.ReadClassPointer<hkaiStreamingCollection>(br);
            m_aabbTree = des.ReadClassPointer<hkcdDynamicAabbTree>(br);
            m_cutter = des.ReadClassPointer<hkaiNavMeshCutter>(br);
            m_cutAabbTolerance = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkaiStreamingCollection>(bw, m_collection);
            s.WriteClassPointer<hkcdDynamicAabbTree>(bw, m_aabbTree);
            s.WriteClassPointer<hkaiNavMeshCutter>(bw, m_cutter);
            bw.WriteSingle(m_cutAabbTolerance);
            bw.WriteUInt32(0);
        }
    }
}

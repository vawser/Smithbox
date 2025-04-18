using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiStaticTreeNavMeshQueryMediator : hkaiNavMeshQueryMediator
    {
        public override uint Signature { get => 1127882337; }
        
        public hkcdStaticAabbTree m_tree;
        public hkaiNavMesh m_navMesh;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_tree = des.ReadClassPointer<hkcdStaticAabbTree>(br);
            m_navMesh = des.ReadClassPointer<hkaiNavMesh>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkcdStaticAabbTree>(bw, m_tree);
            s.WriteClassPointer<hkaiNavMesh>(bw, m_navMesh);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpPhysicsData : hkReferencedObject
    {
        public override uint Signature { get => 1202244227; }
        
        public hkpWorldCinfo m_worldCinfo;
        public List<hkpPhysicsSystem> m_systems;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_worldCinfo = des.ReadClassPointer<hkpWorldCinfo>(br);
            m_systems = des.ReadClassPointerArray<hkpPhysicsSystem>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkpWorldCinfo>(bw, m_worldCinfo);
            s.WriteClassPointerArray<hkpPhysicsSystem>(bw, m_systems);
        }
    }
}

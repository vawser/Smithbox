using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpCollisionFilterList : hkpCollisionFilter
    {
        public override uint Signature { get => 2828262528; }
        
        public List<hkpCollisionFilter> m_collisionFilters;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_collisionFilters = des.ReadClassPointerArray<hkpCollisionFilter>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkpCollisionFilter>(bw, m_collisionFilters);
        }
    }
}

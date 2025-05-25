using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpPairCollisionFilter : hkpCollisionFilter
    {
        public override uint Signature { get => 2794476151; }
        
        public hkpCollisionFilter m_childFilter;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            m_childFilter = des.ReadClassPointer<hkpCollisionFilter>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            s.WriteClassPointer<hkpCollisionFilter>(bw, m_childFilter);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpConstrainedSystemFilter : hkpCollisionFilter
    {
        public override uint Signature { get => 3723550685; }
        
        public hkpCollisionFilter m_otherFilter;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_otherFilter = des.ReadClassPointer<hkpCollisionFilter>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteClassPointer<hkpCollisionFilter>(bw, m_otherFilter);
        }
    }
}

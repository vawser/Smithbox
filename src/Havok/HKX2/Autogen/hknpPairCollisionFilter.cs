using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpPairCollisionFilter : hknpCollisionFilter
    {
        public override uint Signature { get => 817452534; }
        
        public hknpCollisionFilter m_childFilter;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            m_childFilter = des.ReadClassPointer<hknpCollisionFilter>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            s.WriteClassPointer<hknpCollisionFilter>(bw, m_childFilter);
        }
    }
}

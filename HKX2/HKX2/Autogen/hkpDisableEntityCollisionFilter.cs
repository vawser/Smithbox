using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpDisableEntityCollisionFilter : hkpCollisionFilter
    {
        public override uint Signature { get => 4148424604; }
        
        public List<hkpEntity> m_disabledEntities;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_disabledEntities = des.ReadClassPointerArray<hkpEntity>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteClassPointerArray<hkpEntity>(bw, m_disabledEntities);
        }
    }
}

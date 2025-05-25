using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpArrayAction : hkpAction
    {
        public override uint Signature { get => 1419270593; }
        
        public List<hkpEntity> m_entities;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_entities = des.ReadClassPointerArray<hkpEntity>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkpEntity>(bw, m_entities);
        }
    }
}

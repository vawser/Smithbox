using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiAvoidancePairProperties : hkReferencedObject
    {
        public override uint Signature { get => 2677096424; }
        
        public List<hkaiAvoidancePairPropertiesPairData> m_avoidancePairDataMap;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_avoidancePairDataMap = des.ReadClassArray<hkaiAvoidancePairPropertiesPairData>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkaiAvoidancePairPropertiesPairData>(bw, m_avoidancePairDataMap);
        }
    }
}

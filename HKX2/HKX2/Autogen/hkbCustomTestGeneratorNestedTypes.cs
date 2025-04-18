using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCustomTestGeneratorNestedTypes : hkbCustomTestGeneratorNestedTypesBase
    {
        public override uint Signature { get => 1360972261; }
        
        public hkbCustomTestGeneratorNestedTypesBase m_nestedTypeStruct;
        public List<hkbCustomTestGeneratorNestedTypesBase> m_nestedTypeArrayStruct;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_nestedTypeStruct = new hkbCustomTestGeneratorNestedTypesBase();
            m_nestedTypeStruct.Read(des, br);
            m_nestedTypeArrayStruct = des.ReadClassArray<hkbCustomTestGeneratorNestedTypesBase>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_nestedTypeStruct.Write(s, bw);
            s.WriteClassArray<hkbCustomTestGeneratorNestedTypesBase>(bw, m_nestedTypeArrayStruct);
        }
    }
}

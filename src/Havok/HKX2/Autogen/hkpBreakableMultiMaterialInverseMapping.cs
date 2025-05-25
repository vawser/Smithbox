using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpBreakableMultiMaterialInverseMapping : hkReferencedObject
    {
        public override uint Signature { get => 2179142267; }
        
        public List<hkpBreakableMultiMaterialInverseMappingDescriptor> m_descriptors;
        public List<uint> m_subShapeIds;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_descriptors = des.ReadClassArray<hkpBreakableMultiMaterialInverseMappingDescriptor>(br);
            m_subShapeIds = des.ReadUInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkpBreakableMultiMaterialInverseMappingDescriptor>(bw, m_descriptors);
            s.WriteUInt32Array(bw, m_subShapeIds);
        }
    }
}

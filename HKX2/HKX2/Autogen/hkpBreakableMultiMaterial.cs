using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpBreakableMultiMaterial : hkpBreakableMaterial
    {
        public override uint Signature { get => 2869840868; }
        
        public List<hkpBreakableMaterial> m_subMaterials;
        public hkpBreakableMultiMaterialInverseMapping m_inverseMapping;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_subMaterials = des.ReadClassPointerArray<hkpBreakableMaterial>(br);
            m_inverseMapping = des.ReadClassPointer<hkpBreakableMultiMaterialInverseMapping>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkpBreakableMaterial>(bw, m_subMaterials);
            s.WriteClassPointer<hkpBreakableMultiMaterialInverseMapping>(bw, m_inverseMapping);
        }
    }
}

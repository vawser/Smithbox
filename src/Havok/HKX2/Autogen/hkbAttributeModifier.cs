using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbAttributeModifier : hkbModifier
    {
        public override uint Signature { get => 1471316216; }
        
        public List<hkbAttributeModifierAssignment> m_assignments;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_assignments = des.ReadClassArray<hkbAttributeModifierAssignment>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbAttributeModifierAssignment>(bw, m_assignments);
        }
    }
}

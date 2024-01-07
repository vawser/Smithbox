using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbModifierList : hkbModifier
    {
        public override uint Signature { get => 233657932; }
        
        public List<hkbModifier> m_modifiers;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_modifiers = des.ReadClassPointerArray<hkbModifier>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkbModifier>(bw, m_modifiers);
        }
    }
}

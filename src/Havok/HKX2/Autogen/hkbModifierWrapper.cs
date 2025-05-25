using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbModifierWrapper : hkbModifier
    {
        public override uint Signature { get => 4268773620; }
        
        public hkbModifier m_modifier;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_modifier = des.ReadClassPointer<hkbModifier>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbModifier>(bw, m_modifier);
        }
    }
}

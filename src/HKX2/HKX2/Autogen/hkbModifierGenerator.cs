using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbModifierGenerator : hkbGenerator
    {
        public override uint Signature { get => 3298426014; }
        
        public hkbModifier m_modifier;
        public hkbGenerator m_generator;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_modifier = des.ReadClassPointer<hkbModifier>(br);
            m_generator = des.ReadClassPointer<hkbGenerator>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbModifier>(bw, m_modifier);
            s.WriteClassPointer<hkbGenerator>(bw, m_generator);
        }
    }
}

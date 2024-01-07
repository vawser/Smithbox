using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpBreakableMaterial : hkReferencedObject
    {
        public override uint Signature { get => 545189131; }
        
        public float m_strength;
        public int m_typeAndFlags;
        public hkRefCountedProperties m_properties;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_strength = br.ReadSingle();
            m_typeAndFlags = br.ReadInt32();
            m_properties = des.ReadClassPointer<hkRefCountedProperties>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_strength);
            bw.WriteInt32(m_typeAndFlags);
            s.WriteClassPointer<hkRefCountedProperties>(bw, m_properties);
        }
    }
}

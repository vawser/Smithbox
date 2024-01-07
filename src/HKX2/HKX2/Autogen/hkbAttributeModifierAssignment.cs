using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbAttributeModifierAssignment : IHavokObject
    {
        public virtual uint Signature { get => 1220062546; }
        
        public int m_attributeIndex;
        public float m_attributeValue;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_attributeIndex = br.ReadInt32();
            m_attributeValue = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_attributeIndex);
            bw.WriteSingle(m_attributeValue);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBendLinkConstraintSetLink : IHavokObject
    {
        public virtual uint Signature { get => 818040381; }
        
        public ushort m_particleA;
        public ushort m_particleB;
        public float m_bendMinLength;
        public float m_stretchMaxLength;
        public float m_bendStiffness;
        public float m_stretchStiffness;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_particleA = br.ReadUInt16();
            m_particleB = br.ReadUInt16();
            m_bendMinLength = br.ReadSingle();
            m_stretchMaxLength = br.ReadSingle();
            m_bendStiffness = br.ReadSingle();
            m_stretchStiffness = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_particleA);
            bw.WriteUInt16(m_particleB);
            bw.WriteSingle(m_bendMinLength);
            bw.WriteSingle(m_stretchMaxLength);
            bw.WriteSingle(m_bendStiffness);
            bw.WriteSingle(m_stretchStiffness);
        }
    }
}

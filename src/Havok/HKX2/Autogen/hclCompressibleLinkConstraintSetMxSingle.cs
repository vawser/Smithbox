using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclCompressibleLinkConstraintSetMxSingle : IHavokObject
    {
        public virtual uint Signature { get => 3348086667; }
        
        public float m_restLength;
        public float m_compressionLength;
        public float m_stiffnessA;
        public float m_stiffnessB;
        public ushort m_particleA;
        public ushort m_particleB;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_restLength = br.ReadSingle();
            m_compressionLength = br.ReadSingle();
            m_stiffnessA = br.ReadSingle();
            m_stiffnessB = br.ReadSingle();
            m_particleA = br.ReadUInt16();
            m_particleB = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_restLength);
            bw.WriteSingle(m_compressionLength);
            bw.WriteSingle(m_stiffnessA);
            bw.WriteSingle(m_stiffnessB);
            bw.WriteUInt16(m_particleA);
            bw.WriteUInt16(m_particleB);
        }
    }
}

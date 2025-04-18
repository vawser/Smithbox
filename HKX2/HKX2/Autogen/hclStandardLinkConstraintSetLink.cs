using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStandardLinkConstraintSetLink : IHavokObject
    {
        public virtual uint Signature { get => 2145577102; }
        
        public ushort m_particleA;
        public ushort m_particleB;
        public float m_restLength;
        public float m_stiffness;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_particleA = br.ReadUInt16();
            m_particleB = br.ReadUInt16();
            m_restLength = br.ReadSingle();
            m_stiffness = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_particleA);
            bw.WriteUInt16(m_particleB);
            bw.WriteSingle(m_restLength);
            bw.WriteSingle(m_stiffness);
        }
    }
}

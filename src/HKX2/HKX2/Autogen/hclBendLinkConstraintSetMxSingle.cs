using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBendLinkConstraintSetMxSingle : IHavokObject
    {
        public virtual uint Signature { get => 691369221; }
        
        public float m_bendMinLength;
        public float m_stretchMaxLength;
        public float m_stretchStiffness;
        public float m_bendStiffness;
        public float m_invMassA;
        public float m_invMassB;
        public ushort m_particleA;
        public ushort m_particleB;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_bendMinLength = br.ReadSingle();
            m_stretchMaxLength = br.ReadSingle();
            m_stretchStiffness = br.ReadSingle();
            m_bendStiffness = br.ReadSingle();
            m_invMassA = br.ReadSingle();
            m_invMassB = br.ReadSingle();
            m_particleA = br.ReadUInt16();
            m_particleB = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_bendMinLength);
            bw.WriteSingle(m_stretchMaxLength);
            bw.WriteSingle(m_stretchStiffness);
            bw.WriteSingle(m_bendStiffness);
            bw.WriteSingle(m_invMassA);
            bw.WriteSingle(m_invMassB);
            bw.WriteUInt16(m_particleA);
            bw.WriteUInt16(m_particleB);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBendStiffnessConstraintSetMxSingle : IHavokObject
    {
        public virtual uint Signature { get => 3311664447; }
        
        public float m_weightA;
        public float m_weightB;
        public float m_weightC;
        public float m_weightD;
        public float m_bendStiffness;
        public float m_restCurvature;
        public float m_invMassA;
        public float m_invMassB;
        public float m_invMassC;
        public float m_invMassD;
        public ushort m_particleA;
        public ushort m_particleB;
        public ushort m_particleC;
        public ushort m_particleD;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_weightA = br.ReadSingle();
            m_weightB = br.ReadSingle();
            m_weightC = br.ReadSingle();
            m_weightD = br.ReadSingle();
            m_bendStiffness = br.ReadSingle();
            m_restCurvature = br.ReadSingle();
            m_invMassA = br.ReadSingle();
            m_invMassB = br.ReadSingle();
            m_invMassC = br.ReadSingle();
            m_invMassD = br.ReadSingle();
            m_particleA = br.ReadUInt16();
            m_particleB = br.ReadUInt16();
            m_particleC = br.ReadUInt16();
            m_particleD = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_weightA);
            bw.WriteSingle(m_weightB);
            bw.WriteSingle(m_weightC);
            bw.WriteSingle(m_weightD);
            bw.WriteSingle(m_bendStiffness);
            bw.WriteSingle(m_restCurvature);
            bw.WriteSingle(m_invMassA);
            bw.WriteSingle(m_invMassB);
            bw.WriteSingle(m_invMassC);
            bw.WriteSingle(m_invMassD);
            bw.WriteUInt16(m_particleA);
            bw.WriteUInt16(m_particleB);
            bw.WriteUInt16(m_particleC);
            bw.WriteUInt16(m_particleD);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBendStiffnessConstraintSetLink : IHavokObject
    {
        public virtual uint Signature { get => 2341101167; }
        
        public float m_weightA;
        public float m_weightB;
        public float m_weightC;
        public float m_weightD;
        public float m_bendStiffness;
        public float m_restCurvature;
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
            bw.WriteUInt16(m_particleA);
            bw.WriteUInt16(m_particleB);
            bw.WriteUInt16(m_particleC);
            bw.WriteUInt16(m_particleD);
        }
    }
}

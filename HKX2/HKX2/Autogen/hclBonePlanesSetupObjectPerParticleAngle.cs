using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBonePlanesSetupObjectPerParticleAngle : IHavokObject
    {
        public virtual uint Signature { get => 1064119090; }
        
        public string m_transformName;
        public hclVertexSelectionInput m_particlesMaxAngle;
        public hclVertexSelectionInput m_particlesMinAngle;
        public Vector4 m_originBoneSpace;
        public Vector4 m_axisBoneSpace;
        public hclVertexFloatInput m_minAngle;
        public hclVertexFloatInput m_maxAngle;
        public hclVertexFloatInput m_stiffness;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transformName = des.ReadStringPointer(br);
            m_particlesMaxAngle = new hclVertexSelectionInput();
            m_particlesMaxAngle.Read(des, br);
            m_particlesMinAngle = new hclVertexSelectionInput();
            m_particlesMinAngle.Read(des, br);
            br.ReadUInt64();
            m_originBoneSpace = des.ReadVector4(br);
            m_axisBoneSpace = des.ReadVector4(br);
            m_minAngle = new hclVertexFloatInput();
            m_minAngle.Read(des, br);
            m_maxAngle = new hclVertexFloatInput();
            m_maxAngle.Read(des, br);
            m_stiffness = new hclVertexFloatInput();
            m_stiffness.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_transformName);
            m_particlesMaxAngle.Write(s, bw);
            m_particlesMinAngle.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_originBoneSpace);
            s.WriteVector4(bw, m_axisBoneSpace);
            m_minAngle.Write(s, bw);
            m_maxAngle.Write(s, bw);
            m_stiffness.Write(s, bw);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBonePlanesSetupObjectPerParticlePlane : IHavokObject
    {
        public virtual uint Signature { get => 2260926782; }
        
        public string m_transformName;
        public hclVertexSelectionInput m_particles;
        public Vector4 m_directionBoneSpace;
        public hclVertexFloatInput m_allowedDistance;
        public hclVertexFloatInput m_stiffness;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transformName = des.ReadStringPointer(br);
            m_particles = new hclVertexSelectionInput();
            m_particles.Read(des, br);
            br.ReadUInt64();
            m_directionBoneSpace = des.ReadVector4(br);
            m_allowedDistance = new hclVertexFloatInput();
            m_allowedDistance.Read(des, br);
            m_stiffness = new hclVertexFloatInput();
            m_stiffness.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_transformName);
            m_particles.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_directionBoneSpace);
            m_allowedDistance.Write(s, bw);
            m_stiffness.Write(s, bw);
        }
    }
}

using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSimClothDataParticleData : IHavokObject
    {
        public virtual uint Signature { get => 588024872; }
        
        public float m_mass;
        public float m_invMass;
        public float m_radius;
        public float m_friction;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_mass = br.ReadSingle();
            m_invMass = br.ReadSingle();
            m_radius = br.ReadSingle();
            m_friction = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_mass);
            bw.WriteSingle(m_invMass);
            bw.WriteSingle(m_radius);
            bw.WriteSingle(m_friction);
        }
    }
}

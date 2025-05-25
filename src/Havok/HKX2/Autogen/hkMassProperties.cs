using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMassProperties : IHavokObject
    {
        public virtual uint Signature { get => 1755670580; }
        
        public float m_volume;
        public float m_mass;
        public Vector4 m_centerOfMass;
        public Matrix4x4 m_inertiaTensor;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_volume = br.ReadSingle();
            m_mass = br.ReadSingle();
            br.ReadUInt64();
            m_centerOfMass = des.ReadVector4(br);
            m_inertiaTensor = des.ReadMatrix3(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_volume);
            bw.WriteSingle(m_mass);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_centerOfMass);
            s.WriteMatrix3(bw, m_inertiaTensor);
        }
    }
}

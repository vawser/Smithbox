using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiAvoidanceSolverSphereObstacle : IHavokObject
    {
        public virtual uint Signature { get => 4015293709; }
        
        public hkSphere m_sphere;
        public Vector4 m_velocity;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_sphere = new hkSphere();
            m_sphere.Read(des, br);
            m_velocity = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_sphere.Write(s, bw);
            s.WriteVector4(bw, m_velocity);
        }
    }
}

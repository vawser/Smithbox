using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpVehicleLinearCastWheelCollideWheelState : IHavokObject
    {
        public virtual uint Signature { get => 1069210085; }
        
        public hkAabb m_aabb;
        public hknpShape m_shape;
        public Matrix4x4 m_transform;
        public Vector4 m_to;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_aabb = new hkAabb();
            m_aabb.Read(des, br);
            m_shape = des.ReadClassPointer<hknpShape>(br);
            br.ReadUInt64();
            m_transform = des.ReadTransform(br);
            m_to = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_aabb.Write(s, bw);
            s.WriteClassPointer<hknpShape>(bw, m_shape);
            bw.WriteUInt64(0);
            s.WriteTransform(bw, m_transform);
            s.WriteVector4(bw, m_to);
        }
    }
}

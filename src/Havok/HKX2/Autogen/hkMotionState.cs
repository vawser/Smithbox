using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMotionState : IHavokObject
    {
        public virtual uint Signature { get => 1816915289; }
        
        public Matrix4x4 m_transform;
        public Vector4 m_sweptTransform_0;
        public Vector4 m_sweptTransform_1;
        public Vector4 m_sweptTransform_2;
        public Vector4 m_sweptTransform_3;
        public Vector4 m_sweptTransform_4;
        public Vector4 m_deltaAngle;
        public float m_objectRadius;
        public short m_linearDamping;
        public short m_angularDamping;
        public short m_timeFactor;
        public hkUFloat8 m_maxLinearVelocity;
        public hkUFloat8 m_maxAngularVelocity;
        public byte m_deactivationClass;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transform = des.ReadTransform(br);
            m_sweptTransform_0 = des.ReadVector4(br);
            m_sweptTransform_1 = des.ReadVector4(br);
            m_sweptTransform_2 = des.ReadVector4(br);
            m_sweptTransform_3 = des.ReadVector4(br);
            m_sweptTransform_4 = des.ReadVector4(br);
            m_deltaAngle = des.ReadVector4(br);
            m_objectRadius = br.ReadSingle();
            m_linearDamping = br.ReadInt16();
            m_angularDamping = br.ReadInt16();
            m_timeFactor = br.ReadInt16();
            m_maxLinearVelocity = new hkUFloat8();
            m_maxLinearVelocity.Read(des, br);
            m_maxAngularVelocity = new hkUFloat8();
            m_maxAngularVelocity.Read(des, br);
            m_deactivationClass = br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteTransform(bw, m_transform);
            s.WriteVector4(bw, m_sweptTransform_0);
            s.WriteVector4(bw, m_sweptTransform_1);
            s.WriteVector4(bw, m_sweptTransform_2);
            s.WriteVector4(bw, m_sweptTransform_3);
            s.WriteVector4(bw, m_sweptTransform_4);
            s.WriteVector4(bw, m_deltaAngle);
            bw.WriteSingle(m_objectRadius);
            bw.WriteInt16(m_linearDamping);
            bw.WriteInt16(m_angularDamping);
            bw.WriteInt16(m_timeFactor);
            m_maxLinearVelocity.Write(s, bw);
            m_maxAngularVelocity.Write(s, bw);
            bw.WriteByte(m_deactivationClass);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}

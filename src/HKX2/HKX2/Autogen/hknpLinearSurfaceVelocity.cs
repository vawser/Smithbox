using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ProjectMethod
    {
        VELOCITY_PROJECT = 0,
        VELOCITY_RESCALE = 1,
    }
    
    public partial class hknpLinearSurfaceVelocity : hknpSurfaceVelocity
    {
        public override uint Signature { get => 277252529; }
        
        public Space m_space;
        public ProjectMethod m_projectMethod;
        public float m_maxVelocityScale;
        public Vector4 m_velocityMeasurePlane;
        public Vector4 m_velocity;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_space = (Space)br.ReadByte();
            m_projectMethod = (ProjectMethod)br.ReadByte();
            br.ReadUInt16();
            m_maxVelocityScale = br.ReadSingle();
            br.ReadUInt64();
            m_velocityMeasurePlane = des.ReadVector4(br);
            m_velocity = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte((byte)m_space);
            bw.WriteByte((byte)m_projectMethod);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_maxVelocityScale);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_velocityMeasurePlane);
            s.WriteVector4(bw, m_velocity);
        }
    }
}

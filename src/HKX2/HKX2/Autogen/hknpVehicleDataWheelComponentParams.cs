using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpVehicleDataWheelComponentParams : IHavokObject
    {
        public virtual uint Signature { get => 2197700832; }
        
        public float m_radius;
        public float m_mass;
        public float m_width;
        public float m_friction;
        public float m_viscosityFriction;
        public float m_maxFriction;
        public float m_slipAngle;
        public float m_forceFeedbackMultiplier;
        public float m_maxContactBodyAcceleration;
        public sbyte m_axle;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_radius = br.ReadSingle();
            m_mass = br.ReadSingle();
            m_width = br.ReadSingle();
            m_friction = br.ReadSingle();
            m_viscosityFriction = br.ReadSingle();
            m_maxFriction = br.ReadSingle();
            m_slipAngle = br.ReadSingle();
            m_forceFeedbackMultiplier = br.ReadSingle();
            m_maxContactBodyAcceleration = br.ReadSingle();
            m_axle = br.ReadSByte();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_radius);
            bw.WriteSingle(m_mass);
            bw.WriteSingle(m_width);
            bw.WriteSingle(m_friction);
            bw.WriteSingle(m_viscosityFriction);
            bw.WriteSingle(m_maxFriction);
            bw.WriteSingle(m_slipAngle);
            bw.WriteSingle(m_forceFeedbackMultiplier);
            bw.WriteSingle(m_maxContactBodyAcceleration);
            bw.WriteSByte(m_axle);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}

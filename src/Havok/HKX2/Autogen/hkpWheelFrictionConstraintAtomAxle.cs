using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpWheelFrictionConstraintAtomAxle : IHavokObject
    {
        public virtual uint Signature { get => 3645960876; }
        
        public float m_spinVelocity;
        public float m_sumVelocity;
        public int m_numWheels;
        public int m_wheelsSolved;
        public int m_stepsSolved;
        public float m_invInertia;
        public float m_inertia;
        public float m_impulseScaling;
        public float m_impulseMax;
        public bool m_isFixed;
        public int m_numWheelsOnGround;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_spinVelocity = br.ReadSingle();
            m_sumVelocity = br.ReadSingle();
            m_numWheels = br.ReadInt32();
            m_wheelsSolved = br.ReadInt32();
            m_stepsSolved = br.ReadInt32();
            m_invInertia = br.ReadSingle();
            m_inertia = br.ReadSingle();
            m_impulseScaling = br.ReadSingle();
            m_impulseMax = br.ReadSingle();
            m_isFixed = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_numWheelsOnGround = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_spinVelocity);
            bw.WriteSingle(m_sumVelocity);
            bw.WriteInt32(m_numWheels);
            bw.WriteInt32(m_wheelsSolved);
            bw.WriteInt32(m_stepsSolved);
            bw.WriteSingle(m_invInertia);
            bw.WriteSingle(m_inertia);
            bw.WriteSingle(m_impulseScaling);
            bw.WriteSingle(m_impulseMax);
            bw.WriteBoolean(m_isFixed);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_numWheelsOnGround);
        }
    }
}

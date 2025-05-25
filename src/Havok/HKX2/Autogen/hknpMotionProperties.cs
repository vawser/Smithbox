using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum SolverStabilizationType
    {
        SOLVER_STABILIZATION_OFF = 0,
        SOLVER_STABILIZATION_LOW = 1,
        SOLVER_STABILIZATION_MEDIUM = 2,
        SOLVER_STABILIZATION_HIGH = 3,
        SOLVER_STABILIZATION_AGGRESSIVE = 4,
    }
    
    public enum DeactivationStrategy
    {
        DEACTIVATION_STRATEGY_AGGRESSIVE = 3,
        DEACTIVATION_STRATEGY_BALANCED = 4,
        DEACTIVATION_STRATEGY_ACCURATE = 5,
    }
    
    public partial class hknpMotionProperties : IHavokObject
    {
        public virtual uint Signature { get => 1575913025; }
        
        public enum FlagsEnum
        {
            NEVER_REBUILD_MASS_PROPERTIES = 2,
            ENABLE_GRAVITY_MODIFICATION = 536870912,
            ENABLE_TIME_FACTOR = 1073741824,
            FLAGS_MASK = -536870912,
            AUTO_FLAGS_MASK = 66060288,
        }
        
        public uint m_isExclusive;
        public uint m_flags;
        public float m_gravityFactor;
        public float m_timeFactor;
        public float m_maxLinearSpeed;
        public float m_maxAngularSpeed;
        public float m_linearDamping;
        public float m_angularDamping;
        public float m_solverStabilizationSpeedThreshold;
        public float m_solverStabilizationSpeedReduction;
        public float m_maxDistSqrd;
        public float m_maxRotSqrd;
        public float m_invBlockSize;
        public short m_pathingUpperThreshold;
        public short m_pathingLowerThreshold;
        public byte m_numDeactivationFrequencyPasses;
        public byte m_deactivationVelocityScaleSquare;
        public byte m_minimumPathingVelocityScaleSquare;
        public byte m_spikingVelocityScaleThresholdSquared;
        public byte m_minimumSpikingVelocityScaleSquared;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_isExclusive = br.ReadUInt32();
            m_flags = br.ReadUInt32();
            m_gravityFactor = br.ReadSingle();
            m_timeFactor = br.ReadSingle();
            m_maxLinearSpeed = br.ReadSingle();
            m_maxAngularSpeed = br.ReadSingle();
            m_linearDamping = br.ReadSingle();
            m_angularDamping = br.ReadSingle();
            m_solverStabilizationSpeedThreshold = br.ReadSingle();
            m_solverStabilizationSpeedReduction = br.ReadSingle();
            m_maxDistSqrd = br.ReadSingle();
            m_maxRotSqrd = br.ReadSingle();
            m_invBlockSize = br.ReadSingle();
            m_pathingUpperThreshold = br.ReadInt16();
            m_pathingLowerThreshold = br.ReadInt16();
            m_numDeactivationFrequencyPasses = br.ReadByte();
            m_deactivationVelocityScaleSquare = br.ReadByte();
            m_minimumPathingVelocityScaleSquare = br.ReadByte();
            m_spikingVelocityScaleThresholdSquared = br.ReadByte();
            m_minimumSpikingVelocityScaleSquared = br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_isExclusive);
            bw.WriteUInt32(m_flags);
            bw.WriteSingle(m_gravityFactor);
            bw.WriteSingle(m_timeFactor);
            bw.WriteSingle(m_maxLinearSpeed);
            bw.WriteSingle(m_maxAngularSpeed);
            bw.WriteSingle(m_linearDamping);
            bw.WriteSingle(m_angularDamping);
            bw.WriteSingle(m_solverStabilizationSpeedThreshold);
            bw.WriteSingle(m_solverStabilizationSpeedReduction);
            bw.WriteSingle(m_maxDistSqrd);
            bw.WriteSingle(m_maxRotSqrd);
            bw.WriteSingle(m_invBlockSize);
            bw.WriteInt16(m_pathingUpperThreshold);
            bw.WriteInt16(m_pathingLowerThreshold);
            bw.WriteByte(m_numDeactivationFrequencyPasses);
            bw.WriteByte(m_deactivationVelocityScaleSquare);
            bw.WriteByte(m_minimumPathingVelocityScaleSquare);
            bw.WriteByte(m_spikingVelocityScaleThresholdSquared);
            bw.WriteByte(m_minimumSpikingVelocityScaleSquared);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}

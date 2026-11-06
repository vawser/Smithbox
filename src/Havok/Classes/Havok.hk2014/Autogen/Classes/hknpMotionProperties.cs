// Automatically Generated

namespace Havok.hk2014;

public class hknpMotionProperties : IHavokObject
{
    public uint m_isExclusive;

    public hknpMotionProperties.FlagsEnum m_flags;

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


    [Flags]
    public enum FlagsEnum : int
    {
        NEVER_REBUILD_MASS_PROPERTIES = 2,
        ENABLE_GRAVITY_MODIFICATION = 536870912,
        ENABLE_TIME_FACTOR = 1073741824,
        FLAGS_MASK = -536870912,
        AUTO_FLAGS_MASK = 66060288
    }

}

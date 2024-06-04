// Automatically Generated

namespace HKLib.hk2018;

public class hknpMotionProperties : IHavokObject
{
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

    public hknpMotionProperties.DeactivationSettings m_deactivationSettings = new();

    public hknpMotionProperties.FullCastSettings m_fullCastSettings = new();


    public enum SolverStabilizationType : int
    {
        SOLVER_STABILIZATION_OFF = 0,
        SOLVER_STABILIZATION_LOW = 1,
        SOLVER_STABILIZATION_MEDIUM = 2,
        SOLVER_STABILIZATION_HIGH = 3,
        SOLVER_STABILIZATION_AGGRESSIVE = 4
    }

    public class FullCastSettings : IHavokObject
    {
        public float m_minSeparation;

        public float m_minExtraSeparation;

        public float m_toiSeparation;

        public float m_toiExtraSeparation;

        public float m_toiAccuracy;

        public float m_relativeSafeDeltaTime;

        public float m_absoluteSafeDeltaTime;

        public float m_keepTime;

        public float m_keepDistance;

        public int m_maxIterations;

    }


    public class DeactivationSettings : IHavokObject
    {
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


        public enum Strategy : int
        {
            STRATEGY_AGGRESSIVE = 3,
            STRATEGY_BALANCED = 4,
            STRATEGY_ACCURATE = 5
        }

    }


}


// Automatically Generated

namespace HKLib.hk2018;

public class hknpWorldCinfo : IHavokObject
{
    public int m_bodyBufferCapacity;

    public int m_motionBufferCapacity;

    public int m_constraintBufferCapacity;

    public int m_constraintGroupBufferCapacity;

    public bool m_useBodyBacklinkBuffer;

    public hknpMaterialLibrary? m_materialLibrary;

    public hknpMotionPropertiesLibrary? m_motionPropertiesLibrary;

    public hknpBodyQualityLibrary? m_qualityLibrary;

    public byte m_simulationType;

    public int m_numSplitterCells;

    public Vector4 m_gravity = new();

    public float m_airDensity;

    public bool m_enableContactCaching;

    public bool m_mergeEventsBeforeDispatch;

    public byte m_broadPhaseType;

    public hkAabb m_broadPhaseAabb = new();

    public hknpBroadPhaseConfig? m_broadPhaseConfig;

    public hknpCollisionFilter? m_collisionFilter;

    public hknpShapeTagCodec? m_shapeTagCodec;

    public float m_collisionTolerance;

    public float m_relativeCollisionAccuracy;

    public float m_aabbMargin;

    public bool m_enableWeldingForDefaultObjects;

    public bool m_enableWeldingForCriticalObjects;

    public hknpWeldingConfig m_weldingConfig = new();

    public hknpLodManagerCinfo m_lodManagerCinfo = new();

    public bool m_enableSdfEdgeCollisions;

    public bool m_enableCollideWorkStealing;

    public int m_particlesLandscapeQuadCacheSize;

    public float m_solverTau;

    public float m_solverDamp;

    public int m_solverIterations;

    public int m_solverMicrosteps;

    public bool m_enableDeactivation;

    public bool m_enablePenetrationRecovery;

    public float m_maxApproachSpeedForHighQualitySolver;

    public hknpBodyIntegrator? m_bodyIntegrator;

    public bool m_adjustSolverSettingsBasedOnTimestep;

    public float m_expectedDeltaTime;

    public int m_minSolverIterations;

    public int m_maxSolverIterations;


    public enum SolverType : int
    {
        SOLVER_TYPE_INVALID = 0,
        SOLVER_TYPE_2ITERS_SOFT = 1,
        SOLVER_TYPE_2ITERS_MEDIUM = 2,
        SOLVER_TYPE_2ITERS_HARD = 3,
        SOLVER_TYPE_4ITERS_SOFT = 4,
        SOLVER_TYPE_4ITERS_MEDIUM = 5,
        SOLVER_TYPE_4ITERS_HARD = 6,
        SOLVER_TYPE_8ITERS_SOFT = 7,
        SOLVER_TYPE_8ITERS_MEDIUM = 8,
        SOLVER_TYPE_8ITERS_HARD = 9,
        SOLVER_TYPE_MAX = 10
    }

}


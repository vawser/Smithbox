// Automatically Generated

namespace Havok.hk2014;

public class hknpWorldCinfo : IHavokObject
{
    public int m_bodyBufferCapacity;

    public int m_motionBufferCapacity;

    public int m_constraintBufferCapacity;

    public hknpMaterialLibrary? m_materialLibrary;

    public hknpMotionPropertiesLibrary? m_motionPropertiesLibrary;

    public hknpBodyQualityLibrary? m_qualityLibrary;

    public hknpWorldCinfo.SimulationType m_simulationType;

    public int m_numSplitterCells;

    public Vector4 m_gravity = new();

    public bool m_enableContactCaching;

    public bool m_mergeEventsBeforeDispatch;

    public hknpWorldCinfo.LeavingBroadPhaseBehavior m_leavingBroadPhaseBehavior;

    public hkAabb m_broadPhaseAabb = new();

    public hknpBroadPhaseConfig? m_broadPhaseConfig;

    public hknpCollisionFilter? m_collisionFilter;

    public hknpShapeTagCodec? m_shapeTagCodec;

    public float m_collisionTolerance;

    public float m_relativeCollisionAccuracy;

    public bool m_enableWeldingForDefaultObjects;

    public bool m_enableWeldingForCriticalObjects;

    public float m_solverTau;

    public float m_solverDamp;

    public int m_solverIterations;

    public int m_solverMicrosteps;

    public float m_defaultSolverTimestep;

    public float m_maxApproachSpeedForHighQualitySolver;

    public bool m_enableDeactivation;

    public bool m_deleteCachesOnDeactivation;

    public int m_largeIslandSize;

    public bool m_enableSolverDynamicScheduling;

    public int m_contactSolverType;

    public float m_unitScale;

    public bool m_applyUnitScaleToStaticConstants;


    public enum SimulationType : int
    {
        SIMULATION_TYPE_SINGLE_THREADED = 0,
        SIMULATION_TYPE_MULTI_THREADED = 1
    }

    public enum LeavingBroadPhaseBehavior : int
    {
        ON_LEAVING_BROAD_PHASE_DO_NOTHING = 0,
        ON_LEAVING_BROAD_PHASE_REMOVE_BODY = 1,
        ON_LEAVING_BROAD_PHASE_FREEZE_BODY = 2
    }

}

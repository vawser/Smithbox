// Automatically Generated

namespace Havok.hk2014;

public class hkpWorldCinfo : hkReferencedObject
{
    public Vector4 m_gravity = new();

    public int m_broadPhaseQuerySize;

    public float m_contactRestingVelocity;

    public hkpWorldCinfo.BroadPhaseType m_broadPhaseType;

    public hkpWorldCinfo.BroadPhaseBorderBehaviour m_broadPhaseBorderBehaviour;

    public bool m_mtPostponeAndSortBroadPhaseBorderCallbacks;

    public hkAabb m_broadPhaseWorldAabb = new();

    public float m_collisionTolerance;

    public hkpCollisionFilter? m_collisionFilter;

    public hkpConvexListFilter? m_convexListFilter;

    public float m_expectedMaxLinearVelocity;

    public int m_sizeOfToiEventQueue;

    public float m_expectedMinPsiDeltaTime;

    public hkWorldMemoryAvailableWatchDog? m_memoryWatchDog;

    public int m_broadPhaseNumMarkers;

    public hkpWorldCinfo.ContactPointGeneration m_contactPointGeneration;

    public bool m_allowToSkipConfirmedCallbacks;

    public float m_solverTau;

    public float m_solverDamp;

    public int m_solverIterations;

    public int m_solverMicrosteps;

    public float m_maxConstraintViolation;

    public bool m_forceCoherentConstraintOrderingInSolver;

    public float m_snapCollisionToConvexEdgeThreshold;

    public float m_snapCollisionToConcaveEdgeThreshold;

    public bool m_enableToiWeldRejection;

    public bool m_enableDeprecatedWelding;

    public float m_iterativeLinearCastEarlyOutDistance;

    public int m_iterativeLinearCastMaxIterations;

    public byte m_deactivationNumInactiveFramesSelectFlag0;

    public byte m_deactivationNumInactiveFramesSelectFlag1;

    public byte m_deactivationIntegrateCounter;

    public bool m_shouldActivateOnRigidBodyTransformChange;

    public float m_deactivationReferenceDistance;

    public float m_toiCollisionResponseRotateNormal;

    public bool m_useCompoundSpuElf;

    public int m_maxSectorsPerMidphaseCollideTask;

    public int m_maxSectorsPerNarrowphaseCollideTask;

    public bool m_processToisMultithreaded;

    public int m_maxEntriesPerToiMidphaseCollideTask;

    public int m_maxEntriesPerToiNarrowphaseCollideTask;

    public int m_maxNumToiCollisionPairsSinglethreaded;

    public float m_numToisTillAllowedPenetrationSimplifiedToi;

    public float m_numToisTillAllowedPenetrationToi;

    public float m_numToisTillAllowedPenetrationToiHigher;

    public float m_numToisTillAllowedPenetrationToiForced;

    public bool m_enableDeactivation;

    public hkpWorldCinfo.SimulationType m_simulationType;

    public bool m_enableSimulationIslands;

    public uint m_minDesiredIslandSize;

    public bool m_processActionsInSingleThread;

    public bool m_allowIntegrationOfIslandsWithoutConstraintsInASeparateJob;

    public float m_frameMarkerPsiSnap;

    public bool m_fireCollisionCallbacks;


    public enum BroadPhaseType : int
    {
        BROADPHASE_TYPE_SAP = 0,
        BROADPHASE_TYPE_TREE = 1,
        BROADPHASE_TYPE_HYBRID = 2
    }

    public enum BroadPhaseBorderBehaviour : int
    {
        BROADPHASE_BORDER_ASSERT = 0,
        BROADPHASE_BORDER_FIX_ENTITY = 1,
        BROADPHASE_BORDER_REMOVE_ENTITY = 2,
        BROADPHASE_BORDER_DO_NOTHING = 3
    }

    public enum ContactPointGeneration : int
    {
        CONTACT_POINT_ACCEPT_ALWAYS = 0,
        CONTACT_POINT_REJECT_DUBIOUS = 1,
        CONTACT_POINT_REJECT_MANY = 2
    }

    public enum SimulationType : int
    {
        SIMULATION_TYPE_INVALID = 0,
        SIMULATION_TYPE_DISCRETE = 1,
        SIMULATION_TYPE_CONTINUOUS = 2,
        SIMULATION_TYPE_MULTITHREADED = 3
    }

}

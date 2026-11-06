// Automatically Generated

namespace Havok.hk2015;

public class hkpWorldCinfo : hkReferencedObject
{
    public Vector4 m_gravity = new();

    public int m_broadPhaseQuerySize;

    public float m_contactRestingVelocity;

    public sbyte m_broadPhaseType;

    public sbyte m_broadPhaseBorderBehaviour;

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

    public sbyte m_contactPointGeneration;

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

    public sbyte m_simulationType;

    public bool m_enableSimulationIslands;

    public uint m_minDesiredIslandSize;

    public bool m_processActionsInSingleThread;

    public bool m_allowIntegrationOfIslandsWithoutConstraintsInASeparateJob;

    public float m_frameMarkerPsiSnap;

    public bool m_fireCollisionCallbacks;

}

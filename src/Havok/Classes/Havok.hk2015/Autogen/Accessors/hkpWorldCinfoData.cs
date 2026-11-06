// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkpWorldCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkpWorldCinfo>
{
    public hkpWorldCinfoData(Havok.hk2015.hkpWorldCinfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_gravity":
            case "gravity":
            {
                value = instance.m_gravity;
                return true;
            }
            case "m_broadPhaseQuerySize":
            case "broadPhaseQuerySize":
            {
                value = instance.m_broadPhaseQuerySize;
                return true;
            }
            case "m_contactRestingVelocity":
            case "contactRestingVelocity":
            {
                value = instance.m_contactRestingVelocity;
                return true;
            }
            case "m_broadPhaseType":
            case "broadPhaseType":
            {
                value = instance.m_broadPhaseType;
                return true;
            }
            case "m_broadPhaseBorderBehaviour":
            case "broadPhaseBorderBehaviour":
            {
                value = instance.m_broadPhaseBorderBehaviour;
                return true;
            }
            case "m_mtPostponeAndSortBroadPhaseBorderCallbacks":
            case "mtPostponeAndSortBroadPhaseBorderCallbacks":
            {
                value = instance.m_mtPostponeAndSortBroadPhaseBorderCallbacks;
                return true;
            }
            case "m_broadPhaseWorldAabb":
            case "broadPhaseWorldAabb":
            {
                value = instance.m_broadPhaseWorldAabb;
                return true;
            }
            case "m_collisionTolerance":
            case "collisionTolerance":
            {
                value = instance.m_collisionTolerance;
                return true;
            }
            case "m_collisionFilter":
            case "collisionFilter":
            {
                value = instance.m_collisionFilter;
                return true;
            }
            case "m_convexListFilter":
            case "convexListFilter":
            {
                value = instance.m_convexListFilter;
                return true;
            }
            case "m_expectedMaxLinearVelocity":
            case "expectedMaxLinearVelocity":
            {
                value = instance.m_expectedMaxLinearVelocity;
                return true;
            }
            case "m_sizeOfToiEventQueue":
            case "sizeOfToiEventQueue":
            {
                value = instance.m_sizeOfToiEventQueue;
                return true;
            }
            case "m_expectedMinPsiDeltaTime":
            case "expectedMinPsiDeltaTime":
            {
                value = instance.m_expectedMinPsiDeltaTime;
                return true;
            }
            case "m_memoryWatchDog":
            case "memoryWatchDog":
            {
                value = instance.m_memoryWatchDog;
                return true;
            }
            case "m_broadPhaseNumMarkers":
            case "broadPhaseNumMarkers":
            {
                value = instance.m_broadPhaseNumMarkers;
                return true;
            }
            case "m_contactPointGeneration":
            case "contactPointGeneration":
            {
                value = instance.m_contactPointGeneration;
                return true;
            }
            case "m_allowToSkipConfirmedCallbacks":
            case "allowToSkipConfirmedCallbacks":
            {
                value = instance.m_allowToSkipConfirmedCallbacks;
                return true;
            }
            case "m_solverTau":
            case "solverTau":
            {
                value = instance.m_solverTau;
                return true;
            }
            case "m_solverDamp":
            case "solverDamp":
            {
                value = instance.m_solverDamp;
                return true;
            }
            case "m_solverIterations":
            case "solverIterations":
            {
                value = instance.m_solverIterations;
                return true;
            }
            case "m_solverMicrosteps":
            case "solverMicrosteps":
            {
                value = instance.m_solverMicrosteps;
                return true;
            }
            case "m_maxConstraintViolation":
            case "maxConstraintViolation":
            {
                value = instance.m_maxConstraintViolation;
                return true;
            }
            case "m_forceCoherentConstraintOrderingInSolver":
            case "forceCoherentConstraintOrderingInSolver":
            {
                value = instance.m_forceCoherentConstraintOrderingInSolver;
                return true;
            }
            case "m_snapCollisionToConvexEdgeThreshold":
            case "snapCollisionToConvexEdgeThreshold":
            {
                value = instance.m_snapCollisionToConvexEdgeThreshold;
                return true;
            }
            case "m_snapCollisionToConcaveEdgeThreshold":
            case "snapCollisionToConcaveEdgeThreshold":
            {
                value = instance.m_snapCollisionToConcaveEdgeThreshold;
                return true;
            }
            case "m_enableToiWeldRejection":
            case "enableToiWeldRejection":
            {
                value = instance.m_enableToiWeldRejection;
                return true;
            }
            case "m_enableDeprecatedWelding":
            case "enableDeprecatedWelding":
            {
                value = instance.m_enableDeprecatedWelding;
                return true;
            }
            case "m_iterativeLinearCastEarlyOutDistance":
            case "iterativeLinearCastEarlyOutDistance":
            {
                value = instance.m_iterativeLinearCastEarlyOutDistance;
                return true;
            }
            case "m_iterativeLinearCastMaxIterations":
            case "iterativeLinearCastMaxIterations":
            {
                value = instance.m_iterativeLinearCastMaxIterations;
                return true;
            }
            case "m_deactivationNumInactiveFramesSelectFlag0":
            case "deactivationNumInactiveFramesSelectFlag0":
            {
                value = instance.m_deactivationNumInactiveFramesSelectFlag0;
                return true;
            }
            case "m_deactivationNumInactiveFramesSelectFlag1":
            case "deactivationNumInactiveFramesSelectFlag1":
            {
                value = instance.m_deactivationNumInactiveFramesSelectFlag1;
                return true;
            }
            case "m_deactivationIntegrateCounter":
            case "deactivationIntegrateCounter":
            {
                value = instance.m_deactivationIntegrateCounter;
                return true;
            }
            case "m_shouldActivateOnRigidBodyTransformChange":
            case "shouldActivateOnRigidBodyTransformChange":
            {
                value = instance.m_shouldActivateOnRigidBodyTransformChange;
                return true;
            }
            case "m_deactivationReferenceDistance":
            case "deactivationReferenceDistance":
            {
                value = instance.m_deactivationReferenceDistance;
                return true;
            }
            case "m_toiCollisionResponseRotateNormal":
            case "toiCollisionResponseRotateNormal":
            {
                value = instance.m_toiCollisionResponseRotateNormal;
                return true;
            }
            case "m_useCompoundSpuElf":
            case "useCompoundSpuElf":
            {
                value = instance.m_useCompoundSpuElf;
                return true;
            }
            case "m_maxSectorsPerMidphaseCollideTask":
            case "maxSectorsPerMidphaseCollideTask":
            {
                value = instance.m_maxSectorsPerMidphaseCollideTask;
                return true;
            }
            case "m_maxSectorsPerNarrowphaseCollideTask":
            case "maxSectorsPerNarrowphaseCollideTask":
            {
                value = instance.m_maxSectorsPerNarrowphaseCollideTask;
                return true;
            }
            case "m_processToisMultithreaded":
            case "processToisMultithreaded":
            {
                value = instance.m_processToisMultithreaded;
                return true;
            }
            case "m_maxEntriesPerToiMidphaseCollideTask":
            case "maxEntriesPerToiMidphaseCollideTask":
            {
                value = instance.m_maxEntriesPerToiMidphaseCollideTask;
                return true;
            }
            case "m_maxEntriesPerToiNarrowphaseCollideTask":
            case "maxEntriesPerToiNarrowphaseCollideTask":
            {
                value = instance.m_maxEntriesPerToiNarrowphaseCollideTask;
                return true;
            }
            case "m_maxNumToiCollisionPairsSinglethreaded":
            case "maxNumToiCollisionPairsSinglethreaded":
            {
                value = instance.m_maxNumToiCollisionPairsSinglethreaded;
                return true;
            }
            case "m_numToisTillAllowedPenetrationSimplifiedToi":
            case "numToisTillAllowedPenetrationSimplifiedToi":
            {
                value = instance.m_numToisTillAllowedPenetrationSimplifiedToi;
                return true;
            }
            case "m_numToisTillAllowedPenetrationToi":
            case "numToisTillAllowedPenetrationToi":
            {
                value = instance.m_numToisTillAllowedPenetrationToi;
                return true;
            }
            case "m_numToisTillAllowedPenetrationToiHigher":
            case "numToisTillAllowedPenetrationToiHigher":
            {
                value = instance.m_numToisTillAllowedPenetrationToiHigher;
                return true;
            }
            case "m_numToisTillAllowedPenetrationToiForced":
            case "numToisTillAllowedPenetrationToiForced":
            {
                value = instance.m_numToisTillAllowedPenetrationToiForced;
                return true;
            }
            case "m_enableDeactivation":
            case "enableDeactivation":
            {
                value = instance.m_enableDeactivation;
                return true;
            }
            case "m_simulationType":
            case "simulationType":
            {
                value = instance.m_simulationType;
                return true;
            }
            case "m_enableSimulationIslands":
            case "enableSimulationIslands":
            {
                value = instance.m_enableSimulationIslands;
                return true;
            }
            case "m_minDesiredIslandSize":
            case "minDesiredIslandSize":
            {
                value = instance.m_minDesiredIslandSize;
                return true;
            }
            case "m_processActionsInSingleThread":
            case "processActionsInSingleThread":
            {
                value = instance.m_processActionsInSingleThread;
                return true;
            }
            case "m_allowIntegrationOfIslandsWithoutConstraintsInASeparateJob":
            case "allowIntegrationOfIslandsWithoutConstraintsInASeparateJob":
            {
                value = instance.m_allowIntegrationOfIslandsWithoutConstraintsInASeparateJob;
                return true;
            }
            case "m_frameMarkerPsiSnap":
            case "frameMarkerPsiSnap":
            {
                value = instance.m_frameMarkerPsiSnap;
                return true;
            }
            case "m_fireCollisionCallbacks":
            case "fireCollisionCallbacks":
            {
                value = instance.m_fireCollisionCallbacks;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_gravity":
            case "gravity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_gravity = castValue;
                return true;
            }
            case "m_broadPhaseQuerySize":
            case "broadPhaseQuerySize":
            {
                if (value is not int castValue) return false;
                instance.m_broadPhaseQuerySize = castValue;
                return true;
            }
            case "m_contactRestingVelocity":
            case "contactRestingVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_contactRestingVelocity = castValue;
                return true;
            }
            case "m_broadPhaseType":
            case "broadPhaseType":
            {
                if (value is not sbyte castValue) return false;
                instance.m_broadPhaseType = castValue;
                return true;
            }
            case "m_broadPhaseBorderBehaviour":
            case "broadPhaseBorderBehaviour":
            {
                if (value is not sbyte castValue) return false;
                instance.m_broadPhaseBorderBehaviour = castValue;
                return true;
            }
            case "m_mtPostponeAndSortBroadPhaseBorderCallbacks":
            case "mtPostponeAndSortBroadPhaseBorderCallbacks":
            {
                if (value is not bool castValue) return false;
                instance.m_mtPostponeAndSortBroadPhaseBorderCallbacks = castValue;
                return true;
            }
            case "m_broadPhaseWorldAabb":
            case "broadPhaseWorldAabb":
            {
                if (value is not Havok.hk2015.hkAabb castValue) return false;
                instance.m_broadPhaseWorldAabb = castValue;
                return true;
            }
            case "m_collisionTolerance":
            case "collisionTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_collisionTolerance = castValue;
                return true;
            }
            case "m_collisionFilter":
            case "collisionFilter":
            {
                if (value is null)
                {
                    instance.m_collisionFilter = default;
                    return true;
                }
                if (value is Havok.hk2015.hkpCollisionFilter castValue)
                {
                    instance.m_collisionFilter = castValue;
                    return true;
                }
                return false;
            }
            case "m_convexListFilter":
            case "convexListFilter":
            {
                if (value is null)
                {
                    instance.m_convexListFilter = default;
                    return true;
                }
                if (value is Havok.hk2015.hkpConvexListFilter castValue)
                {
                    instance.m_convexListFilter = castValue;
                    return true;
                }
                return false;
            }
            case "m_expectedMaxLinearVelocity":
            case "expectedMaxLinearVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_expectedMaxLinearVelocity = castValue;
                return true;
            }
            case "m_sizeOfToiEventQueue":
            case "sizeOfToiEventQueue":
            {
                if (value is not int castValue) return false;
                instance.m_sizeOfToiEventQueue = castValue;
                return true;
            }
            case "m_expectedMinPsiDeltaTime":
            case "expectedMinPsiDeltaTime":
            {
                if (value is not float castValue) return false;
                instance.m_expectedMinPsiDeltaTime = castValue;
                return true;
            }
            case "m_memoryWatchDog":
            case "memoryWatchDog":
            {
                if (value is null)
                {
                    instance.m_memoryWatchDog = default;
                    return true;
                }
                if (value is Havok.hk2015.hkWorldMemoryAvailableWatchDog castValue)
                {
                    instance.m_memoryWatchDog = castValue;
                    return true;
                }
                return false;
            }
            case "m_broadPhaseNumMarkers":
            case "broadPhaseNumMarkers":
            {
                if (value is not int castValue) return false;
                instance.m_broadPhaseNumMarkers = castValue;
                return true;
            }
            case "m_contactPointGeneration":
            case "contactPointGeneration":
            {
                if (value is not sbyte castValue) return false;
                instance.m_contactPointGeneration = castValue;
                return true;
            }
            case "m_allowToSkipConfirmedCallbacks":
            case "allowToSkipConfirmedCallbacks":
            {
                if (value is not bool castValue) return false;
                instance.m_allowToSkipConfirmedCallbacks = castValue;
                return true;
            }
            case "m_solverTau":
            case "solverTau":
            {
                if (value is not float castValue) return false;
                instance.m_solverTau = castValue;
                return true;
            }
            case "m_solverDamp":
            case "solverDamp":
            {
                if (value is not float castValue) return false;
                instance.m_solverDamp = castValue;
                return true;
            }
            case "m_solverIterations":
            case "solverIterations":
            {
                if (value is not int castValue) return false;
                instance.m_solverIterations = castValue;
                return true;
            }
            case "m_solverMicrosteps":
            case "solverMicrosteps":
            {
                if (value is not int castValue) return false;
                instance.m_solverMicrosteps = castValue;
                return true;
            }
            case "m_maxConstraintViolation":
            case "maxConstraintViolation":
            {
                if (value is not float castValue) return false;
                instance.m_maxConstraintViolation = castValue;
                return true;
            }
            case "m_forceCoherentConstraintOrderingInSolver":
            case "forceCoherentConstraintOrderingInSolver":
            {
                if (value is not bool castValue) return false;
                instance.m_forceCoherentConstraintOrderingInSolver = castValue;
                return true;
            }
            case "m_snapCollisionToConvexEdgeThreshold":
            case "snapCollisionToConvexEdgeThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_snapCollisionToConvexEdgeThreshold = castValue;
                return true;
            }
            case "m_snapCollisionToConcaveEdgeThreshold":
            case "snapCollisionToConcaveEdgeThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_snapCollisionToConcaveEdgeThreshold = castValue;
                return true;
            }
            case "m_enableToiWeldRejection":
            case "enableToiWeldRejection":
            {
                if (value is not bool castValue) return false;
                instance.m_enableToiWeldRejection = castValue;
                return true;
            }
            case "m_enableDeprecatedWelding":
            case "enableDeprecatedWelding":
            {
                if (value is not bool castValue) return false;
                instance.m_enableDeprecatedWelding = castValue;
                return true;
            }
            case "m_iterativeLinearCastEarlyOutDistance":
            case "iterativeLinearCastEarlyOutDistance":
            {
                if (value is not float castValue) return false;
                instance.m_iterativeLinearCastEarlyOutDistance = castValue;
                return true;
            }
            case "m_iterativeLinearCastMaxIterations":
            case "iterativeLinearCastMaxIterations":
            {
                if (value is not int castValue) return false;
                instance.m_iterativeLinearCastMaxIterations = castValue;
                return true;
            }
            case "m_deactivationNumInactiveFramesSelectFlag0":
            case "deactivationNumInactiveFramesSelectFlag0":
            {
                if (value is not byte castValue) return false;
                instance.m_deactivationNumInactiveFramesSelectFlag0 = castValue;
                return true;
            }
            case "m_deactivationNumInactiveFramesSelectFlag1":
            case "deactivationNumInactiveFramesSelectFlag1":
            {
                if (value is not byte castValue) return false;
                instance.m_deactivationNumInactiveFramesSelectFlag1 = castValue;
                return true;
            }
            case "m_deactivationIntegrateCounter":
            case "deactivationIntegrateCounter":
            {
                if (value is not byte castValue) return false;
                instance.m_deactivationIntegrateCounter = castValue;
                return true;
            }
            case "m_shouldActivateOnRigidBodyTransformChange":
            case "shouldActivateOnRigidBodyTransformChange":
            {
                if (value is not bool castValue) return false;
                instance.m_shouldActivateOnRigidBodyTransformChange = castValue;
                return true;
            }
            case "m_deactivationReferenceDistance":
            case "deactivationReferenceDistance":
            {
                if (value is not float castValue) return false;
                instance.m_deactivationReferenceDistance = castValue;
                return true;
            }
            case "m_toiCollisionResponseRotateNormal":
            case "toiCollisionResponseRotateNormal":
            {
                if (value is not float castValue) return false;
                instance.m_toiCollisionResponseRotateNormal = castValue;
                return true;
            }
            case "m_useCompoundSpuElf":
            case "useCompoundSpuElf":
            {
                if (value is not bool castValue) return false;
                instance.m_useCompoundSpuElf = castValue;
                return true;
            }
            case "m_maxSectorsPerMidphaseCollideTask":
            case "maxSectorsPerMidphaseCollideTask":
            {
                if (value is not int castValue) return false;
                instance.m_maxSectorsPerMidphaseCollideTask = castValue;
                return true;
            }
            case "m_maxSectorsPerNarrowphaseCollideTask":
            case "maxSectorsPerNarrowphaseCollideTask":
            {
                if (value is not int castValue) return false;
                instance.m_maxSectorsPerNarrowphaseCollideTask = castValue;
                return true;
            }
            case "m_processToisMultithreaded":
            case "processToisMultithreaded":
            {
                if (value is not bool castValue) return false;
                instance.m_processToisMultithreaded = castValue;
                return true;
            }
            case "m_maxEntriesPerToiMidphaseCollideTask":
            case "maxEntriesPerToiMidphaseCollideTask":
            {
                if (value is not int castValue) return false;
                instance.m_maxEntriesPerToiMidphaseCollideTask = castValue;
                return true;
            }
            case "m_maxEntriesPerToiNarrowphaseCollideTask":
            case "maxEntriesPerToiNarrowphaseCollideTask":
            {
                if (value is not int castValue) return false;
                instance.m_maxEntriesPerToiNarrowphaseCollideTask = castValue;
                return true;
            }
            case "m_maxNumToiCollisionPairsSinglethreaded":
            case "maxNumToiCollisionPairsSinglethreaded":
            {
                if (value is not int castValue) return false;
                instance.m_maxNumToiCollisionPairsSinglethreaded = castValue;
                return true;
            }
            case "m_numToisTillAllowedPenetrationSimplifiedToi":
            case "numToisTillAllowedPenetrationSimplifiedToi":
            {
                if (value is not float castValue) return false;
                instance.m_numToisTillAllowedPenetrationSimplifiedToi = castValue;
                return true;
            }
            case "m_numToisTillAllowedPenetrationToi":
            case "numToisTillAllowedPenetrationToi":
            {
                if (value is not float castValue) return false;
                instance.m_numToisTillAllowedPenetrationToi = castValue;
                return true;
            }
            case "m_numToisTillAllowedPenetrationToiHigher":
            case "numToisTillAllowedPenetrationToiHigher":
            {
                if (value is not float castValue) return false;
                instance.m_numToisTillAllowedPenetrationToiHigher = castValue;
                return true;
            }
            case "m_numToisTillAllowedPenetrationToiForced":
            case "numToisTillAllowedPenetrationToiForced":
            {
                if (value is not float castValue) return false;
                instance.m_numToisTillAllowedPenetrationToiForced = castValue;
                return true;
            }
            case "m_enableDeactivation":
            case "enableDeactivation":
            {
                if (value is not bool castValue) return false;
                instance.m_enableDeactivation = castValue;
                return true;
            }
            case "m_simulationType":
            case "simulationType":
            {
                if (value is not sbyte castValue) return false;
                instance.m_simulationType = castValue;
                return true;
            }
            case "m_enableSimulationIslands":
            case "enableSimulationIslands":
            {
                if (value is not bool castValue) return false;
                instance.m_enableSimulationIslands = castValue;
                return true;
            }
            case "m_minDesiredIslandSize":
            case "minDesiredIslandSize":
            {
                if (value is not uint castValue) return false;
                instance.m_minDesiredIslandSize = castValue;
                return true;
            }
            case "m_processActionsInSingleThread":
            case "processActionsInSingleThread":
            {
                if (value is not bool castValue) return false;
                instance.m_processActionsInSingleThread = castValue;
                return true;
            }
            case "m_allowIntegrationOfIslandsWithoutConstraintsInASeparateJob":
            case "allowIntegrationOfIslandsWithoutConstraintsInASeparateJob":
            {
                if (value is not bool castValue) return false;
                instance.m_allowIntegrationOfIslandsWithoutConstraintsInASeparateJob = castValue;
                return true;
            }
            case "m_frameMarkerPsiSnap":
            case "frameMarkerPsiSnap":
            {
                if (value is not float castValue) return false;
                instance.m_frameMarkerPsiSnap = castValue;
                return true;
            }
            case "m_fireCollisionCallbacks":
            case "fireCollisionCallbacks":
            {
                if (value is not bool castValue) return false;
                instance.m_fireCollisionCallbacks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

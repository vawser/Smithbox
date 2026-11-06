// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpWorldCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpWorldCinfo>
{
    public hknpWorldCinfoData(Havok.hk2014.hknpWorldCinfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bodyBufferCapacity":
            case "bodyBufferCapacity":
            {
                value = instance.m_bodyBufferCapacity;
                return true;
            }
            case "m_motionBufferCapacity":
            case "motionBufferCapacity":
            {
                value = instance.m_motionBufferCapacity;
                return true;
            }
            case "m_constraintBufferCapacity":
            case "constraintBufferCapacity":
            {
                value = instance.m_constraintBufferCapacity;
                return true;
            }
            case "m_materialLibrary":
            case "materialLibrary":
            {
                value = instance.m_materialLibrary;
                return true;
            }
            case "m_motionPropertiesLibrary":
            case "motionPropertiesLibrary":
            {
                value = instance.m_motionPropertiesLibrary;
                return true;
            }
            case "m_qualityLibrary":
            case "qualityLibrary":
            {
                value = instance.m_qualityLibrary;
                return true;
            }
            case "m_simulationType":
            case "simulationType":
            {
                value = instance.m_simulationType;
                return true;
            }
            case "m_numSplitterCells":
            case "numSplitterCells":
            {
                value = instance.m_numSplitterCells;
                return true;
            }
            case "m_gravity":
            case "gravity":
            {
                value = instance.m_gravity;
                return true;
            }
            case "m_enableContactCaching":
            case "enableContactCaching":
            {
                value = instance.m_enableContactCaching;
                return true;
            }
            case "m_mergeEventsBeforeDispatch":
            case "mergeEventsBeforeDispatch":
            {
                value = instance.m_mergeEventsBeforeDispatch;
                return true;
            }
            case "m_leavingBroadPhaseBehavior":
            case "leavingBroadPhaseBehavior":
            {
                value = instance.m_leavingBroadPhaseBehavior;
                return true;
            }
            case "m_broadPhaseAabb":
            case "broadPhaseAabb":
            {
                value = instance.m_broadPhaseAabb;
                return true;
            }
            case "m_broadPhaseConfig":
            case "broadPhaseConfig":
            {
                value = instance.m_broadPhaseConfig;
                return true;
            }
            case "m_collisionFilter":
            case "collisionFilter":
            {
                value = instance.m_collisionFilter;
                return true;
            }
            case "m_shapeTagCodec":
            case "shapeTagCodec":
            {
                value = instance.m_shapeTagCodec;
                return true;
            }
            case "m_collisionTolerance":
            case "collisionTolerance":
            {
                value = instance.m_collisionTolerance;
                return true;
            }
            case "m_relativeCollisionAccuracy":
            case "relativeCollisionAccuracy":
            {
                value = instance.m_relativeCollisionAccuracy;
                return true;
            }
            case "m_enableWeldingForDefaultObjects":
            case "enableWeldingForDefaultObjects":
            {
                value = instance.m_enableWeldingForDefaultObjects;
                return true;
            }
            case "m_enableWeldingForCriticalObjects":
            case "enableWeldingForCriticalObjects":
            {
                value = instance.m_enableWeldingForCriticalObjects;
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
            case "m_defaultSolverTimestep":
            case "defaultSolverTimestep":
            {
                value = instance.m_defaultSolverTimestep;
                return true;
            }
            case "m_maxApproachSpeedForHighQualitySolver":
            case "maxApproachSpeedForHighQualitySolver":
            {
                value = instance.m_maxApproachSpeedForHighQualitySolver;
                return true;
            }
            case "m_enableDeactivation":
            case "enableDeactivation":
            {
                value = instance.m_enableDeactivation;
                return true;
            }
            case "m_deleteCachesOnDeactivation":
            case "deleteCachesOnDeactivation":
            {
                value = instance.m_deleteCachesOnDeactivation;
                return true;
            }
            case "m_largeIslandSize":
            case "largeIslandSize":
            {
                value = instance.m_largeIslandSize;
                return true;
            }
            case "m_enableSolverDynamicScheduling":
            case "enableSolverDynamicScheduling":
            {
                value = instance.m_enableSolverDynamicScheduling;
                return true;
            }
            case "m_contactSolverType":
            case "contactSolverType":
            {
                value = instance.m_contactSolverType;
                return true;
            }
            case "m_unitScale":
            case "unitScale":
            {
                value = instance.m_unitScale;
                return true;
            }
            case "m_applyUnitScaleToStaticConstants":
            case "applyUnitScaleToStaticConstants":
            {
                value = instance.m_applyUnitScaleToStaticConstants;
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
            case "m_bodyBufferCapacity":
            case "bodyBufferCapacity":
            {
                if (value is not int castValue) return false;
                instance.m_bodyBufferCapacity = castValue;
                return true;
            }
            case "m_motionBufferCapacity":
            case "motionBufferCapacity":
            {
                if (value is not int castValue) return false;
                instance.m_motionBufferCapacity = castValue;
                return true;
            }
            case "m_constraintBufferCapacity":
            case "constraintBufferCapacity":
            {
                if (value is not int castValue) return false;
                instance.m_constraintBufferCapacity = castValue;
                return true;
            }
            case "m_materialLibrary":
            case "materialLibrary":
            {
                if (value is null)
                {
                    instance.m_materialLibrary = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpMaterialLibrary castValue)
                {
                    instance.m_materialLibrary = castValue;
                    return true;
                }
                return false;
            }
            case "m_motionPropertiesLibrary":
            case "motionPropertiesLibrary":
            {
                if (value is null)
                {
                    instance.m_motionPropertiesLibrary = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpMotionPropertiesLibrary castValue)
                {
                    instance.m_motionPropertiesLibrary = castValue;
                    return true;
                }
                return false;
            }
            case "m_qualityLibrary":
            case "qualityLibrary":
            {
                if (value is null)
                {
                    instance.m_qualityLibrary = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpBodyQualityLibrary castValue)
                {
                    instance.m_qualityLibrary = castValue;
                    return true;
                }
                return false;
            }
            case "m_simulationType":
            case "simulationType":
            {
                if (value is Havok.hk2014.hknpWorldCinfo.SimulationType castValue)
                {
                    instance.m_simulationType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_simulationType = (Havok.hk2014.hknpWorldCinfo.SimulationType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_numSplitterCells":
            case "numSplitterCells":
            {
                if (value is not int castValue) return false;
                instance.m_numSplitterCells = castValue;
                return true;
            }
            case "m_gravity":
            case "gravity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_gravity = castValue;
                return true;
            }
            case "m_enableContactCaching":
            case "enableContactCaching":
            {
                if (value is not bool castValue) return false;
                instance.m_enableContactCaching = castValue;
                return true;
            }
            case "m_mergeEventsBeforeDispatch":
            case "mergeEventsBeforeDispatch":
            {
                if (value is not bool castValue) return false;
                instance.m_mergeEventsBeforeDispatch = castValue;
                return true;
            }
            case "m_leavingBroadPhaseBehavior":
            case "leavingBroadPhaseBehavior":
            {
                if (value is Havok.hk2014.hknpWorldCinfo.LeavingBroadPhaseBehavior castValue)
                {
                    instance.m_leavingBroadPhaseBehavior = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_leavingBroadPhaseBehavior = (Havok.hk2014.hknpWorldCinfo.LeavingBroadPhaseBehavior)byteValue;
                    return true;
                }
                return false;
            }
            case "m_broadPhaseAabb":
            case "broadPhaseAabb":
            {
                if (value is not Havok.hk2014.hkAabb castValue) return false;
                instance.m_broadPhaseAabb = castValue;
                return true;
            }
            case "m_broadPhaseConfig":
            case "broadPhaseConfig":
            {
                if (value is null)
                {
                    instance.m_broadPhaseConfig = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpBroadPhaseConfig castValue)
                {
                    instance.m_broadPhaseConfig = castValue;
                    return true;
                }
                return false;
            }
            case "m_collisionFilter":
            case "collisionFilter":
            {
                if (value is null)
                {
                    instance.m_collisionFilter = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpCollisionFilter castValue)
                {
                    instance.m_collisionFilter = castValue;
                    return true;
                }
                return false;
            }
            case "m_shapeTagCodec":
            case "shapeTagCodec":
            {
                if (value is null)
                {
                    instance.m_shapeTagCodec = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpShapeTagCodec castValue)
                {
                    instance.m_shapeTagCodec = castValue;
                    return true;
                }
                return false;
            }
            case "m_collisionTolerance":
            case "collisionTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_collisionTolerance = castValue;
                return true;
            }
            case "m_relativeCollisionAccuracy":
            case "relativeCollisionAccuracy":
            {
                if (value is not float castValue) return false;
                instance.m_relativeCollisionAccuracy = castValue;
                return true;
            }
            case "m_enableWeldingForDefaultObjects":
            case "enableWeldingForDefaultObjects":
            {
                if (value is not bool castValue) return false;
                instance.m_enableWeldingForDefaultObjects = castValue;
                return true;
            }
            case "m_enableWeldingForCriticalObjects":
            case "enableWeldingForCriticalObjects":
            {
                if (value is not bool castValue) return false;
                instance.m_enableWeldingForCriticalObjects = castValue;
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
            case "m_defaultSolverTimestep":
            case "defaultSolverTimestep":
            {
                if (value is not float castValue) return false;
                instance.m_defaultSolverTimestep = castValue;
                return true;
            }
            case "m_maxApproachSpeedForHighQualitySolver":
            case "maxApproachSpeedForHighQualitySolver":
            {
                if (value is not float castValue) return false;
                instance.m_maxApproachSpeedForHighQualitySolver = castValue;
                return true;
            }
            case "m_enableDeactivation":
            case "enableDeactivation":
            {
                if (value is not bool castValue) return false;
                instance.m_enableDeactivation = castValue;
                return true;
            }
            case "m_deleteCachesOnDeactivation":
            case "deleteCachesOnDeactivation":
            {
                if (value is not bool castValue) return false;
                instance.m_deleteCachesOnDeactivation = castValue;
                return true;
            }
            case "m_largeIslandSize":
            case "largeIslandSize":
            {
                if (value is not int castValue) return false;
                instance.m_largeIslandSize = castValue;
                return true;
            }
            case "m_enableSolverDynamicScheduling":
            case "enableSolverDynamicScheduling":
            {
                if (value is not bool castValue) return false;
                instance.m_enableSolverDynamicScheduling = castValue;
                return true;
            }
            case "m_contactSolverType":
            case "contactSolverType":
            {
                if (value is not int castValue) return false;
                instance.m_contactSolverType = castValue;
                return true;
            }
            case "m_unitScale":
            case "unitScale":
            {
                if (value is not float castValue) return false;
                instance.m_unitScale = castValue;
                return true;
            }
            case "m_applyUnitScaleToStaticConstants":
            case "applyUnitScaleToStaticConstants":
            {
                if (value is not bool castValue) return false;
                instance.m_applyUnitScaleToStaticConstants = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVdbWorldData : HavokData<hknpVdbWorld> 
{
    public hknpVdbWorldData(HavokType type, hknpVdbWorld instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bodyBufferCapacity":
            case "bodyBufferCapacity":
            {
                if (instance.m_bodyBufferCapacity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motionBufferCapacity":
            case "motionBufferCapacity":
            {
                if (instance.m_motionBufferCapacity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constraintBufferCapacity":
            case "constraintBufferCapacity":
            {
                if (instance.m_constraintBufferCapacity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constraintGroupBufferCapacity":
            case "constraintGroupBufferCapacity":
            {
                if (instance.m_constraintGroupBufferCapacity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useBodyBacklinkBuffer":
            case "useBodyBacklinkBuffer":
            {
                if (instance.m_useBodyBacklinkBuffer is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_materialLibrary":
            case "materialLibrary":
            {
                if (instance.m_materialLibrary is null)
                {
                    return true;
                }
                if (instance.m_materialLibrary is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_motionPropertiesLibrary":
            case "motionPropertiesLibrary":
            {
                if (instance.m_motionPropertiesLibrary is null)
                {
                    return true;
                }
                if (instance.m_motionPropertiesLibrary is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_qualityLibrary":
            case "qualityLibrary":
            {
                if (instance.m_qualityLibrary is null)
                {
                    return true;
                }
                if (instance.m_qualityLibrary is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_simulationType":
            case "simulationType":
            {
                if (instance.m_simulationType is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numSplitterCells":
            case "numSplitterCells":
            {
                if (instance.m_numSplitterCells is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gravity":
            case "gravity":
            {
                if (instance.m_gravity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_airDensity":
            case "airDensity":
            {
                if (instance.m_airDensity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableContactCaching":
            case "enableContactCaching":
            {
                if (instance.m_enableContactCaching is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mergeEventsBeforeDispatch":
            case "mergeEventsBeforeDispatch":
            {
                if (instance.m_mergeEventsBeforeDispatch is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_broadPhaseType":
            case "broadPhaseType":
            {
                if (instance.m_broadPhaseType is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_broadPhaseAabb":
            case "broadPhaseAabb":
            {
                if (instance.m_broadPhaseAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_broadPhaseConfig":
            case "broadPhaseConfig":
            {
                if (instance.m_broadPhaseConfig is null)
                {
                    return true;
                }
                if (instance.m_broadPhaseConfig is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_collisionFilter":
            case "collisionFilter":
            {
                if (instance.m_collisionFilter is null)
                {
                    return true;
                }
                if (instance.m_collisionFilter is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_shapeTagCodec":
            case "shapeTagCodec":
            {
                if (instance.m_shapeTagCodec is null)
                {
                    return true;
                }
                if (instance.m_shapeTagCodec is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_collisionTolerance":
            case "collisionTolerance":
            {
                if (instance.m_collisionTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_relativeCollisionAccuracy":
            case "relativeCollisionAccuracy":
            {
                if (instance.m_relativeCollisionAccuracy is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aabbMargin":
            case "aabbMargin":
            {
                if (instance.m_aabbMargin is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableWeldingForDefaultObjects":
            case "enableWeldingForDefaultObjects":
            {
                if (instance.m_enableWeldingForDefaultObjects is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableWeldingForCriticalObjects":
            case "enableWeldingForCriticalObjects":
            {
                if (instance.m_enableWeldingForCriticalObjects is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weldingConfig":
            case "weldingConfig":
            {
                if (instance.m_weldingConfig is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lodManagerCinfo":
            case "lodManagerCinfo":
            {
                if (instance.m_lodManagerCinfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableSdfEdgeCollisions":
            case "enableSdfEdgeCollisions":
            {
                if (instance.m_enableSdfEdgeCollisions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableCollideWorkStealing":
            case "enableCollideWorkStealing":
            {
                if (instance.m_enableCollideWorkStealing is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particlesLandscapeQuadCacheSize":
            case "particlesLandscapeQuadCacheSize":
            {
                if (instance.m_particlesLandscapeQuadCacheSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_solverTau":
            case "solverTau":
            {
                if (instance.m_solverTau is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_solverDamp":
            case "solverDamp":
            {
                if (instance.m_solverDamp is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_solverIterations":
            case "solverIterations":
            {
                if (instance.m_solverIterations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_solverMicrosteps":
            case "solverMicrosteps":
            {
                if (instance.m_solverMicrosteps is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableDeactivation":
            case "enableDeactivation":
            {
                if (instance.m_enableDeactivation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enablePenetrationRecovery":
            case "enablePenetrationRecovery":
            {
                if (instance.m_enablePenetrationRecovery is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxApproachSpeedForHighQualitySolver":
            case "maxApproachSpeedForHighQualitySolver":
            {
                if (instance.m_maxApproachSpeedForHighQualitySolver is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyIntegrator":
            case "bodyIntegrator":
            {
                if (instance.m_bodyIntegrator is null)
                {
                    return true;
                }
                if (instance.m_bodyIntegrator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_adjustSolverSettingsBasedOnTimestep":
            case "adjustSolverSettingsBasedOnTimestep":
            {
                if (instance.m_adjustSolverSettingsBasedOnTimestep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_expectedDeltaTime":
            case "expectedDeltaTime":
            {
                if (instance.m_expectedDeltaTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minSolverIterations":
            case "minSolverIterations":
            {
                if (instance.m_minSolverIterations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxSolverIterations":
            case "maxSolverIterations":
            {
                if (instance.m_maxSolverIterations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
            case "m_constraintGroupBufferCapacity":
            case "constraintGroupBufferCapacity":
            {
                if (value is not int castValue) return false;
                instance.m_constraintGroupBufferCapacity = castValue;
                return true;
            }
            case "m_useBodyBacklinkBuffer":
            case "useBodyBacklinkBuffer":
            {
                if (value is not bool castValue) return false;
                instance.m_useBodyBacklinkBuffer = castValue;
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
                if (value is hknpMaterialLibrary castValue)
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
                if (value is hknpMotionPropertiesLibrary castValue)
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
                if (value is hknpBodyQualityLibrary castValue)
                {
                    instance.m_qualityLibrary = castValue;
                    return true;
                }
                return false;
            }
            case "m_simulationType":
            case "simulationType":
            {
                if (value is not byte castValue) return false;
                instance.m_simulationType = castValue;
                return true;
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
            case "m_airDensity":
            case "airDensity":
            {
                if (value is not float castValue) return false;
                instance.m_airDensity = castValue;
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
            case "m_broadPhaseType":
            case "broadPhaseType":
            {
                if (value is not byte castValue) return false;
                instance.m_broadPhaseType = castValue;
                return true;
            }
            case "m_broadPhaseAabb":
            case "broadPhaseAabb":
            {
                if (value is not hkAabb castValue) return false;
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
                if (value is hknpBroadPhaseConfig castValue)
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
                if (value is hknpCollisionFilter castValue)
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
                if (value is hknpShapeTagCodec castValue)
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
            case "m_aabbMargin":
            case "aabbMargin":
            {
                if (value is not float castValue) return false;
                instance.m_aabbMargin = castValue;
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
            case "m_weldingConfig":
            case "weldingConfig":
            {
                if (value is not hknpWeldingConfig castValue) return false;
                instance.m_weldingConfig = castValue;
                return true;
            }
            case "m_lodManagerCinfo":
            case "lodManagerCinfo":
            {
                if (value is not hknpLodManagerCinfo castValue) return false;
                instance.m_lodManagerCinfo = castValue;
                return true;
            }
            case "m_enableSdfEdgeCollisions":
            case "enableSdfEdgeCollisions":
            {
                if (value is not bool castValue) return false;
                instance.m_enableSdfEdgeCollisions = castValue;
                return true;
            }
            case "m_enableCollideWorkStealing":
            case "enableCollideWorkStealing":
            {
                if (value is not bool castValue) return false;
                instance.m_enableCollideWorkStealing = castValue;
                return true;
            }
            case "m_particlesLandscapeQuadCacheSize":
            case "particlesLandscapeQuadCacheSize":
            {
                if (value is not int castValue) return false;
                instance.m_particlesLandscapeQuadCacheSize = castValue;
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
            case "m_enableDeactivation":
            case "enableDeactivation":
            {
                if (value is not bool castValue) return false;
                instance.m_enableDeactivation = castValue;
                return true;
            }
            case "m_enablePenetrationRecovery":
            case "enablePenetrationRecovery":
            {
                if (value is not bool castValue) return false;
                instance.m_enablePenetrationRecovery = castValue;
                return true;
            }
            case "m_maxApproachSpeedForHighQualitySolver":
            case "maxApproachSpeedForHighQualitySolver":
            {
                if (value is not float castValue) return false;
                instance.m_maxApproachSpeedForHighQualitySolver = castValue;
                return true;
            }
            case "m_bodyIntegrator":
            case "bodyIntegrator":
            {
                if (value is null)
                {
                    instance.m_bodyIntegrator = default;
                    return true;
                }
                if (value is hknpBodyIntegrator castValue)
                {
                    instance.m_bodyIntegrator = castValue;
                    return true;
                }
                return false;
            }
            case "m_adjustSolverSettingsBasedOnTimestep":
            case "adjustSolverSettingsBasedOnTimestep":
            {
                if (value is not bool castValue) return false;
                instance.m_adjustSolverSettingsBasedOnTimestep = castValue;
                return true;
            }
            case "m_expectedDeltaTime":
            case "expectedDeltaTime":
            {
                if (value is not float castValue) return false;
                instance.m_expectedDeltaTime = castValue;
                return true;
            }
            case "m_minSolverIterations":
            case "minSolverIterations":
            {
                if (value is not int castValue) return false;
                instance.m_minSolverIterations = castValue;
                return true;
            }
            case "m_maxSolverIterations":
            case "maxSolverIterations":
            {
                if (value is not int castValue) return false;
                instance.m_maxSolverIterations = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

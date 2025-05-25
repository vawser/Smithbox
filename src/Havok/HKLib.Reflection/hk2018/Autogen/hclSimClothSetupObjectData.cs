// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSimClothSetupObjectData : HavokData<hclSimClothSetupObject> 
{
    public hclSimClothSetupObjectData(HavokType type, hclSimClothSetupObject instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
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
            case "m_simulationMesh":
            case "simulationMesh":
            {
                if (instance.m_simulationMesh is null)
                {
                    return true;
                }
                if (instance.m_simulationMesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_collidableTransformSet":
            case "collidableTransformSet":
            {
                if (instance.m_collidableTransformSet is null)
                {
                    return true;
                }
                if (instance.m_collidableTransformSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_gravity":
            case "gravity":
            {
                if (instance.m_gravity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_globalDampingPerSecond":
            case "globalDampingPerSecond":
            {
                if (instance.m_globalDampingPerSecond is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_doNormals":
            case "doNormals":
            {
                if (instance.m_doNormals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_specifyDensity":
            case "specifyDensity":
            {
                if (instance.m_specifyDensity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexDensity":
            case "vertexDensity":
            {
                if (instance.m_vertexDensity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rescaleMass":
            case "rescaleMass":
            {
                if (instance.m_rescaleMass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_totalMass":
            case "totalMass":
            {
                if (instance.m_totalMass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleMass":
            case "particleMass":
            {
                if (instance.m_particleMass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleRadius":
            case "particleRadius":
            {
                if (instance.m_particleRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleFriction":
            case "particleFriction":
            {
                if (instance.m_particleFriction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fixedParticles":
            case "fixedParticles":
            {
                if (instance.m_fixedParticles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enablePinchDetection":
            case "enablePinchDetection":
            {
                if (instance.m_enablePinchDetection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pinchDetectionEnabledParticles":
            case "pinchDetectionEnabledParticles":
            {
                if (instance.m_pinchDetectionEnabledParticles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toAnimPeriod":
            case "toAnimPeriod":
            {
                if (instance.m_toAnimPeriod is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toSimPeriod":
            case "toSimPeriod":
            {
                if (instance.m_toSimPeriod is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_drivePinchedParticlesToReferenceMesh":
            case "drivePinchedParticlesToReferenceMesh":
            {
                if (instance.m_drivePinchedParticlesToReferenceMesh is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pinchReferenceBufferSetup":
            case "pinchReferenceBufferSetup":
            {
                if (instance.m_pinchReferenceBufferSetup is null)
                {
                    return true;
                }
                if (instance.m_pinchReferenceBufferSetup is TGet castValue)
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
            case "m_landscapeCollisionParticleSelection":
            case "landscapeCollisionParticleSelection":
            {
                if (instance.m_landscapeCollisionParticleSelection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_landscapeCollisionParticleRadius":
            case "landscapeCollisionParticleRadius":
            {
                if (instance.m_landscapeCollisionParticleRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableStuckParticleDetection":
            case "enableStuckParticleDetection":
            {
                if (instance.m_enableStuckParticleDetection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stuckParticlesStretchFactor":
            case "stuckParticlesStretchFactor":
            {
                if (instance.m_stuckParticlesStretchFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableLandscapePinchDetection":
            case "enableLandscapePinchDetection":
            {
                if (instance.m_enableLandscapePinchDetection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_landscapePinchDetectionPriority":
            case "landscapePinchDetectionPriority":
            {
                if (instance.m_landscapePinchDetectionPriority is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_landscapePinchDetectionRadius":
            case "landscapePinchDetectionRadius":
            {
                if (instance.m_landscapePinchDetectionRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableTransferMotion":
            case "enableTransferMotion":
            {
                if (instance.m_enableTransferMotion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transferMotionSetupData":
            case "transferMotionSetupData":
            {
                if (instance.m_transferMotionSetupData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_virtualCollisionPoints":
            case "virtualCollisionPoints":
            {
                if (instance.m_virtualCollisionPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_virtualCollisionPointDensities":
            case "virtualCollisionPointDensities":
            {
                if (instance.m_virtualCollisionPointDensities is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_virtualCollisionPointUseAllCollidables":
            case "virtualCollisionPointUseAllCollidables":
            {
                if (instance.m_virtualCollisionPointUseAllCollidables is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_virtualCollisionPointCollidables":
            case "virtualCollisionPointCollidables":
            {
                if (instance.m_virtualCollisionPointCollidables is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_landscapeVirtualCollisionPoints":
            case "landscapeVirtualCollisionPoints":
            {
                if (instance.m_landscapeVirtualCollisionPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_landscapeVirtualCollisionPointDensities":
            case "landscapeVirtualCollisionPointDensities":
            {
                if (instance.m_landscapeVirtualCollisionPointDensities is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constraintSetSetups":
            case "constraintSetSetups":
            {
                if (instance.m_constraintSetSetups is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_perInstanceCollidables":
            case "perInstanceCollidables":
            {
                if (instance.m_perInstanceCollidables is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
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
            case "m_simulationMesh":
            case "simulationMesh":
            {
                if (value is null)
                {
                    instance.m_simulationMesh = default;
                    return true;
                }
                if (value is hclSimulationSetupMesh castValue)
                {
                    instance.m_simulationMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_collidableTransformSet":
            case "collidableTransformSet":
            {
                if (value is null)
                {
                    instance.m_collidableTransformSet = default;
                    return true;
                }
                if (value is hclTransformSetSetupObject castValue)
                {
                    instance.m_collidableTransformSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_gravity":
            case "gravity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_gravity = castValue;
                return true;
            }
            case "m_globalDampingPerSecond":
            case "globalDampingPerSecond":
            {
                if (value is not float castValue) return false;
                instance.m_globalDampingPerSecond = castValue;
                return true;
            }
            case "m_doNormals":
            case "doNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_doNormals = castValue;
                return true;
            }
            case "m_specifyDensity":
            case "specifyDensity":
            {
                if (value is not bool castValue) return false;
                instance.m_specifyDensity = castValue;
                return true;
            }
            case "m_vertexDensity":
            case "vertexDensity":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_vertexDensity = castValue;
                return true;
            }
            case "m_rescaleMass":
            case "rescaleMass":
            {
                if (value is not bool castValue) return false;
                instance.m_rescaleMass = castValue;
                return true;
            }
            case "m_totalMass":
            case "totalMass":
            {
                if (value is not float castValue) return false;
                instance.m_totalMass = castValue;
                return true;
            }
            case "m_particleMass":
            case "particleMass":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_particleMass = castValue;
                return true;
            }
            case "m_particleRadius":
            case "particleRadius":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_particleRadius = castValue;
                return true;
            }
            case "m_particleFriction":
            case "particleFriction":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_particleFriction = castValue;
                return true;
            }
            case "m_fixedParticles":
            case "fixedParticles":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_fixedParticles = castValue;
                return true;
            }
            case "m_enablePinchDetection":
            case "enablePinchDetection":
            {
                if (value is not bool castValue) return false;
                instance.m_enablePinchDetection = castValue;
                return true;
            }
            case "m_pinchDetectionEnabledParticles":
            case "pinchDetectionEnabledParticles":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_pinchDetectionEnabledParticles = castValue;
                return true;
            }
            case "m_toAnimPeriod":
            case "toAnimPeriod":
            {
                if (value is not float castValue) return false;
                instance.m_toAnimPeriod = castValue;
                return true;
            }
            case "m_toSimPeriod":
            case "toSimPeriod":
            {
                if (value is not float castValue) return false;
                instance.m_toSimPeriod = castValue;
                return true;
            }
            case "m_drivePinchedParticlesToReferenceMesh":
            case "drivePinchedParticlesToReferenceMesh":
            {
                if (value is not bool castValue) return false;
                instance.m_drivePinchedParticlesToReferenceMesh = castValue;
                return true;
            }
            case "m_pinchReferenceBufferSetup":
            case "pinchReferenceBufferSetup":
            {
                if (value is null)
                {
                    instance.m_pinchReferenceBufferSetup = default;
                    return true;
                }
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_pinchReferenceBufferSetup = castValue;
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
            case "m_landscapeCollisionParticleSelection":
            case "landscapeCollisionParticleSelection":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_landscapeCollisionParticleSelection = castValue;
                return true;
            }
            case "m_landscapeCollisionParticleRadius":
            case "landscapeCollisionParticleRadius":
            {
                if (value is not float castValue) return false;
                instance.m_landscapeCollisionParticleRadius = castValue;
                return true;
            }
            case "m_enableStuckParticleDetection":
            case "enableStuckParticleDetection":
            {
                if (value is not bool castValue) return false;
                instance.m_enableStuckParticleDetection = castValue;
                return true;
            }
            case "m_stuckParticlesStretchFactor":
            case "stuckParticlesStretchFactor":
            {
                if (value is not float castValue) return false;
                instance.m_stuckParticlesStretchFactor = castValue;
                return true;
            }
            case "m_enableLandscapePinchDetection":
            case "enableLandscapePinchDetection":
            {
                if (value is not bool castValue) return false;
                instance.m_enableLandscapePinchDetection = castValue;
                return true;
            }
            case "m_landscapePinchDetectionPriority":
            case "landscapePinchDetectionPriority":
            {
                if (value is not sbyte castValue) return false;
                instance.m_landscapePinchDetectionPriority = castValue;
                return true;
            }
            case "m_landscapePinchDetectionRadius":
            case "landscapePinchDetectionRadius":
            {
                if (value is not float castValue) return false;
                instance.m_landscapePinchDetectionRadius = castValue;
                return true;
            }
            case "m_enableTransferMotion":
            case "enableTransferMotion":
            {
                if (value is not bool castValue) return false;
                instance.m_enableTransferMotion = castValue;
                return true;
            }
            case "m_transferMotionSetupData":
            case "transferMotionSetupData":
            {
                if (value is not hclSimClothSetupObject.TransferMotionSetupData castValue) return false;
                instance.m_transferMotionSetupData = castValue;
                return true;
            }
            case "m_virtualCollisionPoints":
            case "virtualCollisionPoints":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_virtualCollisionPoints = castValue;
                return true;
            }
            case "m_virtualCollisionPointDensities":
            case "virtualCollisionPointDensities":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_virtualCollisionPointDensities = castValue;
                return true;
            }
            case "m_virtualCollisionPointUseAllCollidables":
            case "virtualCollisionPointUseAllCollidables":
            {
                if (value is not bool castValue) return false;
                instance.m_virtualCollisionPointUseAllCollidables = castValue;
                return true;
            }
            case "m_virtualCollisionPointCollidables":
            case "virtualCollisionPointCollidables":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_virtualCollisionPointCollidables = castValue;
                return true;
            }
            case "m_landscapeVirtualCollisionPoints":
            case "landscapeVirtualCollisionPoints":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_landscapeVirtualCollisionPoints = castValue;
                return true;
            }
            case "m_landscapeVirtualCollisionPointDensities":
            case "landscapeVirtualCollisionPointDensities":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_landscapeVirtualCollisionPointDensities = castValue;
                return true;
            }
            case "m_constraintSetSetups":
            case "constraintSetSetups":
            {
                if (value is not List<hclConstraintSetSetupObject?> castValue) return false;
                instance.m_constraintSetSetups = castValue;
                return true;
            }
            case "m_perInstanceCollidables":
            case "perInstanceCollidables":
            {
                if (value is not List<hclSimClothSetupObject.PerInstanceCollidable> castValue) return false;
                instance.m_perInstanceCollidables = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

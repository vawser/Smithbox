// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclSimClothSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclSimClothSetupObject>
{
    public hclSimClothSetupObjectData(Havok.hk2018.hclSimClothSetupObject instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_simulationMesh":
            case "simulationMesh":
            {
                value = instance.m_simulationMesh;
                return true;
            }
            case "m_collidableTransformSet":
            case "collidableTransformSet":
            {
                value = instance.m_collidableTransformSet;
                return true;
            }
            case "m_gravity":
            case "gravity":
            {
                value = instance.m_gravity;
                return true;
            }
            case "m_globalDampingPerSecond":
            case "globalDampingPerSecond":
            {
                value = instance.m_globalDampingPerSecond;
                return true;
            }
            case "m_doNormals":
            case "doNormals":
            {
                value = instance.m_doNormals;
                return true;
            }
            case "m_specifyDensity":
            case "specifyDensity":
            {
                value = instance.m_specifyDensity;
                return true;
            }
            case "m_vertexDensity":
            case "vertexDensity":
            {
                value = instance.m_vertexDensity;
                return true;
            }
            case "m_rescaleMass":
            case "rescaleMass":
            {
                value = instance.m_rescaleMass;
                return true;
            }
            case "m_totalMass":
            case "totalMass":
            {
                value = instance.m_totalMass;
                return true;
            }
            case "m_particleMass":
            case "particleMass":
            {
                value = instance.m_particleMass;
                return true;
            }
            case "m_particleRadius":
            case "particleRadius":
            {
                value = instance.m_particleRadius;
                return true;
            }
            case "m_particleFriction":
            case "particleFriction":
            {
                value = instance.m_particleFriction;
                return true;
            }
            case "m_fixedParticles":
            case "fixedParticles":
            {
                value = instance.m_fixedParticles;
                return true;
            }
            case "m_enablePinchDetection":
            case "enablePinchDetection":
            {
                value = instance.m_enablePinchDetection;
                return true;
            }
            case "m_pinchDetectionEnabledParticles":
            case "pinchDetectionEnabledParticles":
            {
                value = instance.m_pinchDetectionEnabledParticles;
                return true;
            }
            case "m_toAnimPeriod":
            case "toAnimPeriod":
            {
                value = instance.m_toAnimPeriod;
                return true;
            }
            case "m_toSimPeriod":
            case "toSimPeriod":
            {
                value = instance.m_toSimPeriod;
                return true;
            }
            case "m_drivePinchedParticlesToReferenceMesh":
            case "drivePinchedParticlesToReferenceMesh":
            {
                value = instance.m_drivePinchedParticlesToReferenceMesh;
                return true;
            }
            case "m_pinchReferenceBufferSetup":
            case "pinchReferenceBufferSetup":
            {
                value = instance.m_pinchReferenceBufferSetup;
                return true;
            }
            case "m_collisionTolerance":
            case "collisionTolerance":
            {
                value = instance.m_collisionTolerance;
                return true;
            }
            case "m_landscapeCollisionParticleSelection":
            case "landscapeCollisionParticleSelection":
            {
                value = instance.m_landscapeCollisionParticleSelection;
                return true;
            }
            case "m_landscapeCollisionParticleRadius":
            case "landscapeCollisionParticleRadius":
            {
                value = instance.m_landscapeCollisionParticleRadius;
                return true;
            }
            case "m_enableStuckParticleDetection":
            case "enableStuckParticleDetection":
            {
                value = instance.m_enableStuckParticleDetection;
                return true;
            }
            case "m_stuckParticlesStretchFactor":
            case "stuckParticlesStretchFactor":
            {
                value = instance.m_stuckParticlesStretchFactor;
                return true;
            }
            case "m_enableLandscapePinchDetection":
            case "enableLandscapePinchDetection":
            {
                value = instance.m_enableLandscapePinchDetection;
                return true;
            }
            case "m_landscapePinchDetectionPriority":
            case "landscapePinchDetectionPriority":
            {
                value = instance.m_landscapePinchDetectionPriority;
                return true;
            }
            case "m_landscapePinchDetectionRadius":
            case "landscapePinchDetectionRadius":
            {
                value = instance.m_landscapePinchDetectionRadius;
                return true;
            }
            case "m_enableTransferMotion":
            case "enableTransferMotion":
            {
                value = instance.m_enableTransferMotion;
                return true;
            }
            case "m_transferMotionSetupData":
            case "transferMotionSetupData":
            {
                value = instance.m_transferMotionSetupData;
                return true;
            }
            case "m_virtualCollisionPoints":
            case "virtualCollisionPoints":
            {
                value = instance.m_virtualCollisionPoints;
                return true;
            }
            case "m_virtualCollisionPointDensities":
            case "virtualCollisionPointDensities":
            {
                value = instance.m_virtualCollisionPointDensities;
                return true;
            }
            case "m_virtualCollisionPointUseAllCollidables":
            case "virtualCollisionPointUseAllCollidables":
            {
                value = instance.m_virtualCollisionPointUseAllCollidables;
                return true;
            }
            case "m_virtualCollisionPointCollidables":
            case "virtualCollisionPointCollidables":
            {
                value = instance.m_virtualCollisionPointCollidables;
                return true;
            }
            case "m_landscapeVirtualCollisionPoints":
            case "landscapeVirtualCollisionPoints":
            {
                value = instance.m_landscapeVirtualCollisionPoints;
                return true;
            }
            case "m_landscapeVirtualCollisionPointDensities":
            case "landscapeVirtualCollisionPointDensities":
            {
                value = instance.m_landscapeVirtualCollisionPointDensities;
                return true;
            }
            case "m_constraintSetSetups":
            case "constraintSetSetups":
            {
                value = instance.m_constraintSetSetups;
                return true;
            }
            case "m_perInstanceCollidables":
            case "perInstanceCollidables":
            {
                value = instance.m_perInstanceCollidables;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
                if (value is Havok.hk2018.hclSimulationSetupMesh castValue)
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
                if (value is Havok.hk2018.hclTransformSetSetupObject castValue)
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
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
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
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
                instance.m_particleMass = castValue;
                return true;
            }
            case "m_particleRadius":
            case "particleRadius":
            {
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
                instance.m_particleRadius = castValue;
                return true;
            }
            case "m_particleFriction":
            case "particleFriction":
            {
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
                instance.m_particleFriction = castValue;
                return true;
            }
            case "m_fixedParticles":
            case "fixedParticles":
            {
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
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
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
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
                if (value is Havok.hk2018.hclBufferSetupObject castValue)
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
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
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
                if (value is not Havok.hk2018.hclSimClothSetupObject.TransferMotionSetupData castValue) return false;
                instance.m_transferMotionSetupData = castValue;
                return true;
            }
            case "m_virtualCollisionPoints":
            case "virtualCollisionPoints":
            {
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
                instance.m_virtualCollisionPoints = castValue;
                return true;
            }
            case "m_virtualCollisionPointDensities":
            case "virtualCollisionPointDensities":
            {
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
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
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
                instance.m_landscapeVirtualCollisionPoints = castValue;
                return true;
            }
            case "m_landscapeVirtualCollisionPointDensities":
            case "landscapeVirtualCollisionPointDensities":
            {
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
                instance.m_landscapeVirtualCollisionPointDensities = castValue;
                return true;
            }
            case "m_constraintSetSetups":
            case "constraintSetSetups":
            {
                if (value is not List<Havok.hk2018.hclConstraintSetSetupObject?> castValue) return false;
                instance.m_constraintSetSetups = castValue;
                return true;
            }
            case "m_perInstanceCollidables":
            case "perInstanceCollidables":
            {
                if (value is not List<Havok.hk2018.hclSimClothSetupObject.PerInstanceCollidable> castValue) return false;
                instance.m_perInstanceCollidables = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

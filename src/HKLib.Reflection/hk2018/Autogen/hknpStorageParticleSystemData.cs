// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hknpLevelOfDetail;
using Enum = HKLib.hk2018.hknpLevelOfDetail.Enum;

namespace HKLib.Reflection.hk2018;

internal class hknpStorageParticleSystemData : HavokData<hknpStorageParticleSystem> 
{
    public hknpStorageParticleSystemData(HavokType type, hknpStorageParticleSystem instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_originalId":
            case "originalId":
            {
                if (instance.m_originalId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                if (instance.m_friction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_restitution":
            case "restitution":
            {
                if (instance.m_restitution is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                if (instance.m_shape is null)
                {
                    return true;
                }
                if (instance.m_shape is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (instance.m_collisionFilterInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rigidBodyCollisionLod":
            case "rigidBodyCollisionLod":
            {
                if (instance.m_rigidBodyCollisionLod is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_rigidBodyCollisionLod is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_enableDynamicBodyCollisions":
            case "enableDynamicBodyCollisions":
            {
                if (instance.m_enableDynamicBodyCollisions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableParticleParticleCollisions":
            case "enableParticleParticleCollisions":
            {
                if (instance.m_enableParticleParticleCollisions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableDeterministicParticleParticleCollisions":
            case "enableDeterministicParticleParticleCollisions":
            {
                if (instance.m_enableDeterministicParticleParticleCollisions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableQueries":
            case "enableQueries":
            {
                if (instance.m_enableQueries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_refitBoundingVolumeAfterStep":
            case "refitBoundingVolumeAfterStep":
            {
                if (instance.m_refitBoundingVolumeAfterStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_raiseParticlesCollidedWithBodiesEvents":
            case "raiseParticlesCollidedWithBodiesEvents":
            {
                if (instance.m_raiseParticlesCollidedWithBodiesEvents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_raiseParticlesCollidedWithParticlesEvents":
            case "raiseParticlesCollidedWithParticlesEvents":
            {
                if (instance.m_raiseParticlesCollidedWithParticlesEvents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_raiseParticleBodyImpulseAppliedEvents":
            case "raiseParticleBodyImpulseAppliedEvents":
            {
                if (instance.m_raiseParticleBodyImpulseAppliedEvents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_raiseParticleParticleImpulseAppliedEvents":
            case "raiseParticleParticleImpulseAppliedEvents":
            {
                if (instance.m_raiseParticleParticleImpulseAppliedEvents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_raiseParticlesExitedBroadPhaseEvents":
            case "raiseParticlesExitedBroadPhaseEvents":
            {
                if (instance.m_raiseParticlesExitedBroadPhaseEvents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_callbackImpulseThreshold":
            case "callbackImpulseThreshold":
            {
                if (instance.m_callbackImpulseThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_supportDisabledParticles":
            case "supportDisabledParticles":
            {
                if (instance.m_supportDisabledParticles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxBatchSize":
            case "maxBatchSize":
            {
                if (instance.m_maxBatchSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numParticles":
            case "numParticles":
            {
                if (instance.m_numParticles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_capacity":
            case "capacity":
            {
                if (instance.m_capacity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_doAngular":
            case "doAngular":
            {
                if (instance.m_doAngular is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_radiusBasedLookahead":
            case "radiusBasedLookahead":
            {
                if (instance.m_radiusBasedLookahead is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocityBasedLookahead":
            case "velocityBasedLookahead":
            {
                if (instance.m_velocityBasedLookahead is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearAccelerationRestingThreshold":
            case "linearAccelerationRestingThreshold":
            {
                if (instance.m_linearAccelerationRestingThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularVelocityRestingThreshold":
            case "angularVelocityRestingThreshold":
            {
                if (instance.m_angularVelocityRestingThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleParticleFriction":
            case "particleParticleFriction":
            {
                if (instance.m_particleParticleFriction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_relativeTimeForPenetrationRecovery":
            case "relativeTimeForPenetrationRecovery":
            {
                if (instance.m_relativeTimeForPenetrationRecovery is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxPenetrationRecoveryImpulse":
            case "maxPenetrationRecoveryImpulse":
            {
                if (instance.m_maxPenetrationRecoveryImpulse is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleShapeIndex":
            case "particleShapeIndex":
            {
                if (instance.m_particleShapeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_positions":
            case "positions":
            {
                if (instance.m_positions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_orientations":
            case "orientations":
            {
                if (instance.m_orientations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearVelocities":
            case "linearVelocities":
            {
                if (instance.m_linearVelocities is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularVelocities":
            case "angularVelocities":
            {
                if (instance.m_angularVelocities is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_frictions":
            case "frictions":
            {
                if (instance.m_frictions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_restitutions":
            case "restitutions":
            {
                if (instance.m_restitutions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shapes":
            case "shapes":
            {
                if (instance.m_shapes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shapeIndices":
            case "shapeIndices":
            {
                if (instance.m_shapeIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enabledParticles":
            case "enabledParticles":
            {
                if (instance.m_enabledParticles is not TGet castValue) return false;
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
            case "m_originalId":
            case "originalId":
            {
                if (value is not hknpParticlesColliderId castValue) return false;
                instance.m_originalId = castValue;
                return true;
            }
            case "m_id":
            case "id":
            {
                if (value is not hknpParticlesColliderId castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                if (value is not float castValue) return false;
                instance.m_friction = castValue;
                return true;
            }
            case "m_restitution":
            case "restitution":
            {
                if (value is not float castValue) return false;
                instance.m_restitution = castValue;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is hknpConvexShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_collisionFilterInfo = castValue;
                return true;
            }
            case "m_rigidBodyCollisionLod":
            case "rigidBodyCollisionLod":
            {
                if (value is Enum castValue)
                {
                    instance.m_rigidBodyCollisionLod = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_rigidBodyCollisionLod = (Enum)intValue;
                    return true;
                }
                return false;
            }
            case "m_enableDynamicBodyCollisions":
            case "enableDynamicBodyCollisions":
            {
                if (value is not bool castValue) return false;
                instance.m_enableDynamicBodyCollisions = castValue;
                return true;
            }
            case "m_enableParticleParticleCollisions":
            case "enableParticleParticleCollisions":
            {
                if (value is not bool castValue) return false;
                instance.m_enableParticleParticleCollisions = castValue;
                return true;
            }
            case "m_enableDeterministicParticleParticleCollisions":
            case "enableDeterministicParticleParticleCollisions":
            {
                if (value is not bool castValue) return false;
                instance.m_enableDeterministicParticleParticleCollisions = castValue;
                return true;
            }
            case "m_enableQueries":
            case "enableQueries":
            {
                if (value is not bool castValue) return false;
                instance.m_enableQueries = castValue;
                return true;
            }
            case "m_refitBoundingVolumeAfterStep":
            case "refitBoundingVolumeAfterStep":
            {
                if (value is not bool castValue) return false;
                instance.m_refitBoundingVolumeAfterStep = castValue;
                return true;
            }
            case "m_raiseParticlesCollidedWithBodiesEvents":
            case "raiseParticlesCollidedWithBodiesEvents":
            {
                if (value is not bool castValue) return false;
                instance.m_raiseParticlesCollidedWithBodiesEvents = castValue;
                return true;
            }
            case "m_raiseParticlesCollidedWithParticlesEvents":
            case "raiseParticlesCollidedWithParticlesEvents":
            {
                if (value is not bool castValue) return false;
                instance.m_raiseParticlesCollidedWithParticlesEvents = castValue;
                return true;
            }
            case "m_raiseParticleBodyImpulseAppliedEvents":
            case "raiseParticleBodyImpulseAppliedEvents":
            {
                if (value is not bool castValue) return false;
                instance.m_raiseParticleBodyImpulseAppliedEvents = castValue;
                return true;
            }
            case "m_raiseParticleParticleImpulseAppliedEvents":
            case "raiseParticleParticleImpulseAppliedEvents":
            {
                if (value is not bool castValue) return false;
                instance.m_raiseParticleParticleImpulseAppliedEvents = castValue;
                return true;
            }
            case "m_raiseParticlesExitedBroadPhaseEvents":
            case "raiseParticlesExitedBroadPhaseEvents":
            {
                if (value is not bool castValue) return false;
                instance.m_raiseParticlesExitedBroadPhaseEvents = castValue;
                return true;
            }
            case "m_callbackImpulseThreshold":
            case "callbackImpulseThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_callbackImpulseThreshold = castValue;
                return true;
            }
            case "m_supportDisabledParticles":
            case "supportDisabledParticles":
            {
                if (value is not bool castValue) return false;
                instance.m_supportDisabledParticles = castValue;
                return true;
            }
            case "m_maxBatchSize":
            case "maxBatchSize":
            {
                if (value is not int castValue) return false;
                instance.m_maxBatchSize = castValue;
                return true;
            }
            case "m_numParticles":
            case "numParticles":
            {
                if (value is not int castValue) return false;
                instance.m_numParticles = castValue;
                return true;
            }
            case "m_capacity":
            case "capacity":
            {
                if (value is not int castValue) return false;
                instance.m_capacity = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_doAngular":
            case "doAngular":
            {
                if (value is not bool castValue) return false;
                instance.m_doAngular = castValue;
                return true;
            }
            case "m_radiusBasedLookahead":
            case "radiusBasedLookahead":
            {
                if (value is not float castValue) return false;
                instance.m_radiusBasedLookahead = castValue;
                return true;
            }
            case "m_velocityBasedLookahead":
            case "velocityBasedLookahead":
            {
                if (value is not float castValue) return false;
                instance.m_velocityBasedLookahead = castValue;
                return true;
            }
            case "m_linearAccelerationRestingThreshold":
            case "linearAccelerationRestingThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_linearAccelerationRestingThreshold = castValue;
                return true;
            }
            case "m_angularVelocityRestingThreshold":
            case "angularVelocityRestingThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_angularVelocityRestingThreshold = castValue;
                return true;
            }
            case "m_particleParticleFriction":
            case "particleParticleFriction":
            {
                if (value is not float castValue) return false;
                instance.m_particleParticleFriction = castValue;
                return true;
            }
            case "m_relativeTimeForPenetrationRecovery":
            case "relativeTimeForPenetrationRecovery":
            {
                if (value is not float castValue) return false;
                instance.m_relativeTimeForPenetrationRecovery = castValue;
                return true;
            }
            case "m_maxPenetrationRecoveryImpulse":
            case "maxPenetrationRecoveryImpulse":
            {
                if (value is not float castValue) return false;
                instance.m_maxPenetrationRecoveryImpulse = castValue;
                return true;
            }
            case "m_particleShapeIndex":
            case "particleShapeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_particleShapeIndex = castValue;
                return true;
            }
            case "m_positions":
            case "positions":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_positions = castValue;
                return true;
            }
            case "m_orientations":
            case "orientations":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_orientations = castValue;
                return true;
            }
            case "m_linearVelocities":
            case "linearVelocities":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_linearVelocities = castValue;
                return true;
            }
            case "m_angularVelocities":
            case "angularVelocities":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_angularVelocities = castValue;
                return true;
            }
            case "m_frictions":
            case "frictions":
            {
                if (value is not List<float> castValue) return false;
                instance.m_frictions = castValue;
                return true;
            }
            case "m_restitutions":
            case "restitutions":
            {
                if (value is not List<float> castValue) return false;
                instance.m_restitutions = castValue;
                return true;
            }
            case "m_shapes":
            case "shapes":
            {
                if (value is not List<hknpConvexShape?> castValue) return false;
                instance.m_shapes = castValue;
                return true;
            }
            case "m_shapeIndices":
            case "shapeIndices":
            {
                if (value is not List<int> castValue) return false;
                instance.m_shapeIndices = castValue;
                return true;
            }
            case "m_enabledParticles":
            case "enabledParticles":
            {
                if (value is not List<bool> castValue) return false;
                instance.m_enabledParticles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

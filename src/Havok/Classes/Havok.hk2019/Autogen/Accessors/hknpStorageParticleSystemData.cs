// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpStorageParticleSystemData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpStorageParticleSystem>
{
    public hknpStorageParticleSystemData(Havok.hk2019.hknpStorageParticleSystem instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_originalId":
            case "originalId":
            {
                value = instance.m_originalId;
                return true;
            }
            case "m_id":
            case "id":
            {
                value = instance.m_id;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                value = instance.m_friction;
                return true;
            }
            case "m_restitution":
            case "restitution":
            {
                value = instance.m_restitution;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_filterData":
            case "filterData":
            {
                value = instance.m_filterData;
                return true;
            }
            case "m_rigidBodyCollisionLod":
            case "rigidBodyCollisionLod":
            {
                value = instance.m_rigidBodyCollisionLod;
                return true;
            }
            case "m_enableDynamicBodyCollisions":
            case "enableDynamicBodyCollisions":
            {
                value = instance.m_enableDynamicBodyCollisions;
                return true;
            }
            case "m_enableParticleParticleCollisions":
            case "enableParticleParticleCollisions":
            {
                value = instance.m_enableParticleParticleCollisions;
                return true;
            }
            case "m_enableDeterministicParticleParticleCollisions":
            case "enableDeterministicParticleParticleCollisions":
            {
                value = instance.m_enableDeterministicParticleParticleCollisions;
                return true;
            }
            case "m_enableQueries":
            case "enableQueries":
            {
                value = instance.m_enableQueries;
                return true;
            }
            case "m_raiseParticlesCollidedWithBodiesEvents":
            case "raiseParticlesCollidedWithBodiesEvents":
            {
                value = instance.m_raiseParticlesCollidedWithBodiesEvents;
                return true;
            }
            case "m_raiseParticlesCollidedWithParticlesEvents":
            case "raiseParticlesCollidedWithParticlesEvents":
            {
                value = instance.m_raiseParticlesCollidedWithParticlesEvents;
                return true;
            }
            case "m_raiseParticleBodyImpulseAppliedEvents":
            case "raiseParticleBodyImpulseAppliedEvents":
            {
                value = instance.m_raiseParticleBodyImpulseAppliedEvents;
                return true;
            }
            case "m_raiseParticleParticleImpulseAppliedEvents":
            case "raiseParticleParticleImpulseAppliedEvents":
            {
                value = instance.m_raiseParticleParticleImpulseAppliedEvents;
                return true;
            }
            case "m_raiseParticlesExitedBroadPhaseEvents":
            case "raiseParticlesExitedBroadPhaseEvents":
            {
                value = instance.m_raiseParticlesExitedBroadPhaseEvents;
                return true;
            }
            case "m_callbackImpulseThreshold":
            case "callbackImpulseThreshold":
            {
                value = instance.m_callbackImpulseThreshold;
                return true;
            }
            case "m_supportDisabledParticles":
            case "supportDisabledParticles":
            {
                value = instance.m_supportDisabledParticles;
                return true;
            }
            case "m_solverMicroSteps":
            case "solverMicroSteps":
            {
                value = instance.m_solverMicroSteps;
                return true;
            }
            case "m_maxParticleDomain":
            case "maxParticleDomain":
            {
                value = instance.m_maxParticleDomain;
                return true;
            }
            case "m_maxBatchSize":
            case "maxBatchSize":
            {
                value = instance.m_maxBatchSize;
                return true;
            }
            case "m_numParticles":
            case "numParticles":
            {
                value = instance.m_numParticles;
                return true;
            }
            case "m_capacity":
            case "capacity":
            {
                value = instance.m_capacity;
                return true;
            }
            case "m_isAddedToWorld":
            case "isAddedToWorld":
            {
                value = instance.m_isAddedToWorld;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_doAngular":
            case "doAngular":
            {
                value = instance.m_doAngular;
                return true;
            }
            case "m_radiusBasedLookahead":
            case "radiusBasedLookahead":
            {
                value = instance.m_radiusBasedLookahead;
                return true;
            }
            case "m_velocityBasedLookahead":
            case "velocityBasedLookahead":
            {
                value = instance.m_velocityBasedLookahead;
                return true;
            }
            case "m_linearAccelerationRestingThreshold":
            case "linearAccelerationRestingThreshold":
            {
                value = instance.m_linearAccelerationRestingThreshold;
                return true;
            }
            case "m_angularVelocityRestingThreshold":
            case "angularVelocityRestingThreshold":
            {
                value = instance.m_angularVelocityRestingThreshold;
                return true;
            }
            case "m_particleParticleFriction":
            case "particleParticleFriction":
            {
                value = instance.m_particleParticleFriction;
                return true;
            }
            case "m_relativeTimeForPenetrationRecovery":
            case "relativeTimeForPenetrationRecovery":
            {
                value = instance.m_relativeTimeForPenetrationRecovery;
                return true;
            }
            case "m_maxPenetrationRecoveryImpulse":
            case "maxPenetrationRecoveryImpulse":
            {
                value = instance.m_maxPenetrationRecoveryImpulse;
                return true;
            }
            case "m_particleShapeIndex":
            case "particleShapeIndex":
            {
                value = instance.m_particleShapeIndex;
                return true;
            }
            case "m_positions":
            case "positions":
            {
                value = instance.m_positions;
                return true;
            }
            case "m_orientations":
            case "orientations":
            {
                value = instance.m_orientations;
                return true;
            }
            case "m_linearVelocities":
            case "linearVelocities":
            {
                value = instance.m_linearVelocities;
                return true;
            }
            case "m_angularVelocities":
            case "angularVelocities":
            {
                value = instance.m_angularVelocities;
                return true;
            }
            case "m_frictions":
            case "frictions":
            {
                value = instance.m_frictions;
                return true;
            }
            case "m_restitutions":
            case "restitutions":
            {
                value = instance.m_restitutions;
                return true;
            }
            case "m_shapes":
            case "shapes":
            {
                value = instance.m_shapes;
                return true;
            }
            case "m_shapeIndices":
            case "shapeIndices":
            {
                value = instance.m_shapeIndices;
                return true;
            }
            case "m_enabledParticles":
            case "enabledParticles":
            {
                value = instance.m_enabledParticles;
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
            case "m_originalId":
            case "originalId":
            {
                if (value is not Havok.hk2019.hknpParticlesColliderId castValue) return false;
                instance.m_originalId = castValue;
                return true;
            }
            case "m_id":
            case "id":
            {
                if (value is not Havok.hk2019.hknpParticlesColliderId castValue) return false;
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
                if (value is Havok.hk2019.hknpConvexShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_filterData":
            case "filterData":
            {
                if (value is not Havok.hk2019.hknpQueryFilterData castValue) return false;
                instance.m_filterData = castValue;
                return true;
            }
            case "m_rigidBodyCollisionLod":
            case "rigidBodyCollisionLod":
            {
                if (value is Havok.hk2019.hknpLevelOfDetail castValue)
                {
                    instance.m_rigidBodyCollisionLod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_rigidBodyCollisionLod = (Havok.hk2019.hknpLevelOfDetail)byteValue;
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
            case "m_solverMicroSteps":
            case "solverMicroSteps":
            {
                if (value is not int castValue) return false;
                instance.m_solverMicroSteps = castValue;
                return true;
            }
            case "m_maxParticleDomain":
            case "maxParticleDomain":
            {
                if (value is not float castValue) return false;
                instance.m_maxParticleDomain = castValue;
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
            case "m_isAddedToWorld":
            case "isAddedToWorld":
            {
                if (value is not bool castValue) return false;
                instance.m_isAddedToWorld = castValue;
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
                if (value is not List<Havok.hk2019.hknpConvexShape?> castValue) return false;
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

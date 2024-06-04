// Automatically Generated

namespace HKLib.hk2018;

public class hknpStorageParticleSystem : IHavokObject
{
    public hknpParticlesColliderId m_originalId = new();

    public hknpParticlesColliderId m_id = new();

    public float m_friction;

    public float m_restitution;

    public hknpConvexShape? m_shape;

    public uint m_collisionFilterInfo;

    public hknpLevelOfDetail.Enum m_rigidBodyCollisionLod;

    public bool m_enableDynamicBodyCollisions;

    public bool m_enableParticleParticleCollisions;

    public bool m_enableDeterministicParticleParticleCollisions;

    public bool m_enableQueries;

    public bool m_refitBoundingVolumeAfterStep;

    public bool m_raiseParticlesCollidedWithBodiesEvents;

    public bool m_raiseParticlesCollidedWithParticlesEvents;

    public bool m_raiseParticleBodyImpulseAppliedEvents;

    public bool m_raiseParticleParticleImpulseAppliedEvents;

    public bool m_raiseParticlesExitedBroadPhaseEvents;

    public float m_callbackImpulseThreshold;

    public bool m_supportDisabledParticles;

    public int m_maxBatchSize;

    public int m_numParticles;

    public int m_capacity;

    public ulong m_userData;

    public bool m_doAngular;

    public float m_radiusBasedLookahead;

    public float m_velocityBasedLookahead;

    public float m_linearAccelerationRestingThreshold;

    public float m_angularVelocityRestingThreshold;

    public float m_particleParticleFriction;

    public float m_relativeTimeForPenetrationRecovery;

    public float m_maxPenetrationRecoveryImpulse;

    public int m_particleShapeIndex;

    public List<Vector4> m_positions = new();

    public List<Vector4> m_orientations = new();

    public List<Vector4> m_linearVelocities = new();

    public List<Vector4> m_angularVelocities = new();

    public List<float> m_frictions = new();

    public List<float> m_restitutions = new();

    public List<hknpConvexShape?> m_shapes = new();

    public List<int> m_shapeIndices = new();

    public List<bool> m_enabledParticles = new();

}


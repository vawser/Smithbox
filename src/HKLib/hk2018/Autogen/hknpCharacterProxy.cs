// Automatically Generated

namespace HKLib.hk2018;

public class hknpCharacterProxy : hkReferencedObject
{
    public hknpShape? m_shape;

    public ulong m_userData;

    public hknpBodyId m_bodyId = new();

    public uint m_collisionFilterInfo;

    public Matrix4x4 m_transform = new();

    public hkAabb m_aabb = new();

    public Vector4 m_velocity = new();

    public Vector4 m_lastDisplacement = new();

    public Vector4 m_lastVelocity = new();

    public float m_lastInvDeltaTime;

    public float m_maxSlopeCosine;

    public float m_dynamicFriction;

    public float m_staticFriction;

    public Vector4 m_up = new();

    public float m_keepDistance;

    public float m_keepContactTolerance;

    public float m_contactAngleSensitivity;

    public int m_userPlanes;

    public float m_maxCharacterSpeedForSolver;

    public float m_characterStrength;

    public float m_characterMass;

    public float m_penetrationRecoverySpeed;

    public int m_maxCastIterations;

    public bool m_refreshManifoldInCheckSupport;

}


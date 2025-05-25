// Automatically Generated

namespace HKLib.hk2018;

public class hclSimClothInstance : hkReferencedObject
{
    public float m_lastTimeStepUsed;

    public float m_effectiveGlobalDampingInv;

    public hkAabb m_particlesAabb = new();

    public hkAabb m_collisionParticlesAabb = new();

    public bool m_updateCollisionAabbs;

    public hkAabb m_landscapeCollisionParticlesAabb = new();

    public Matrix4x4 m_previousTransferWorldFromModel = new();

    public hclConstraintStiffnessDispatcher.Type m_constraintStiffnessModelType;

    public float m_slowMotionFactor;

    public float m_worldSteppingFactor;

    public ulong m_userData;

    public bool m_transferMotionEnabled;

    public bool m_landscapeCollisionEnabled;

    public bool m_pinchDetectionEnabled;

    public uint m_virtualCollisionPointsFlags;

}


// Automatically Generated

namespace Havok.hk55;

public class hkMotionState : IHavokObject
{
    public Matrix4x4 m_transform = new();

    public hkSweptTransform m_sweptTransform = new();

    public Vector4 m_deltaAngle = new();

    public float m_objectRadius;

    public float m_linearDamping;

    public float m_angularDamping;

    public byte m_maxLinearVelocity;

    public byte m_maxAngularVelocity;

    public byte m_deactivationClass;

}

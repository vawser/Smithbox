// Automatically Generated

namespace Havok.hk2015;

public class hkMotionState : IHavokObject
{
    public Matrix4x4 m_transform = new();

    public readonly Vector4[] m_sweptTransform = new Vector4[5];

    public Vector4 m_deltaAngle = new();

    public float m_objectRadius;

    public float m_linearDamping;

    public float m_angularDamping;

    public float m_timeFactor;

    public hkUFloat8 m_maxLinearVelocity = new();

    public hkUFloat8 m_maxAngularVelocity = new();

    public byte m_deactivationClass;

}

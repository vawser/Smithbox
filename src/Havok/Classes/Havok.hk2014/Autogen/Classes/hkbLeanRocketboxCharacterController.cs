// Automatically Generated

namespace Havok.hk2014;

public class hkbLeanRocketboxCharacterController : hkbModifier
{
    public hkbGenerator? m_child;

    public int m_desiredAIMovementMode;

    public float m_effectiveLinearSpeed;

    public float m_effectiveAngularSpeed;

    public float m_effectiveHorizontalAim;

    public float m_effectiveVerticalAim;

    public float m_torsoTiltAngle;

    public float m_desiredAIMovementSpeed;

    public float m_currentMaximumSpeed;

    public float m_linearSpeed;

    public float m_angularSpeed;

    public float m_horizontalAim;

    public float m_verticalAim;

    public float m_rotationSpeed;

    public int m_poseIdx;

    public int m_rotationAllowed;

    public hkbEventProperty m_leftFootDownEvent = new();

    public hkbEventProperty m_rightFootDownEvent = new();

    public hkbEventProperty m_immediateStopEvent = new();

    public hkbEventProperty m_changePoseEvent = new();

    public hkbEventProperty m_moveEvent = new();

    public hkbEventProperty m_stopEvent = new();

    public List<float> m_moveVelocities = new();

}

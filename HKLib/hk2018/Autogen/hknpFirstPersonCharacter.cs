// Automatically Generated

namespace HKLib.hk2018;

public class hknpFirstPersonCharacter : hkReferencedObject
{
    public float m_verticalSensitivity;

    public float m_horizontalSensitivity;

    public float m_sensivityPadX;

    public float m_sensivityPadY;

    public float m_eyeHeight;

    public float m_gravityStrength;

    public float m_maxUpDownAngle;

    public uint m_numFramesPerShot;

    public float m_forwardBackwardSpeedModifier;

    public float m_leftRightSpeedModifier;

    public uint m_flags;

    public int m_gunCounter;

    public int m_gunCounterRmb;

    public float m_currentAngle;

    public float m_currentElevation;

    public bool m_specialGravity;

    public Vector4 m_gravity = new();

    public hknpFirstPersonGun? m_currentGun;


    public enum ControlFlags : int
    {
        NO_FLAGS = 0,
        CAN_DETACH_FROM_CHAR = 1,
        HAS_USER_CONTROL = 2,
        MAKE_OCCLUDING_OBJECTS_TRANSPARENT = 4,
        DISABLE_JUMP = 8,
        INVERT_UP_DOWN = 16,
        DISABLE_DPAD = 32,
        DISABLE_CAM_CONTROL = 64
    }

}


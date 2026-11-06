// Automatically Generated

namespace Havok.hk2014;

public class hkbCharacterControllerModifier : hkbModifier
{
    public hkbCharacterControllerModifierControlData m_controlData = new();

    public Vector4 m_initialVelocity = new();

    public hkbCharacterControllerModifier.InitialVelocityCoordinates m_initialVelocityCoordinates;

    public hkbCharacterControllerModifier.MotionMode m_motionMode;

    public bool m_forceDownwardMomentum;

    public bool m_applyGravity;

    public bool m_setInitialVelocity;

    public bool m_isTouchingGround;


    public enum InitialVelocityCoordinates : int
    {
        INITIAL_VELOCITY_IN_WORLD_COORDINATES = 0,
        INITIAL_VELOCITY_IN_MODEL_COORDINATES = 1
    }

    public enum MotionMode : int
    {
        MOTION_MODE_FOLLOW_ANIMATION = 0,
        MOTION_MODE_DYNAMIC = 1
    }

}

// Automatically Generated

namespace HKLib.hk2018;

public class hkbCharacterControllerModifier : hkbModifier, hkbVerifiable
{
    public hkbCharacterControllerModifierControlData m_controlData = new();

    public Vector4 m_initialVelocity = new();

    public hkbCharacterControllerModifier.InitialVelocityCoordinates m_initialVelocityCoordinates;

    public hkbCharacterControllerModifier.MotionMode m_motionMode;

    public float m_gravityFactor;

    public bool m_setInitialVelocity;

    public bool m_isTouchingGround;

    public int m_collisionShapeProfileIdx;


    public enum MotionMode : int
    {
        MOTION_MODE_FOLLOW_ANIMATION = 0,
        MOTION_MODE_DYNAMIC = 1
    }

    public enum InitialVelocityCoordinates : int
    {
        INITIAL_VELOCITY_IN_WORLD_COORDINATES = 0,
        INITIAL_VELOCITY_IN_MODEL_COORDINATES = 1
    }

}


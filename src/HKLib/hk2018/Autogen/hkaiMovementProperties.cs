// Automatically Generated

namespace HKLib.hk2018;

public class hkaiMovementProperties : IHavokObject
{
    public float m_minVelocity;

    public float m_maxVelocity;

    public float m_maxAcceleration;

    public float m_maxDeceleration;

    public float m_leftTurnRadius;

    public float m_rightTurnRadius;

    public float m_maxAngularVelocity;

    public float m_maxTurnVelocity;

    public hkaiMovementProperties.KinematicConstraintType m_kinematicConstraintType;


    public enum KinematicConstraintType : int
    {
        CONSTRAINTS_NONE = 0,
        CONSTRAINTS_LINEAR_AND_ANGULAR = 1,
        CONSTRAINTS_LINEAR_ONLY = 2
    }

}


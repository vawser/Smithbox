// Automatically Generated

namespace HKLib.hk2018;

public class hkaiLocalSteeringInput : IHavokObject
{
    public Vector4 m_currentPosition = new();

    public Vector4 m_currentForward = new();

    public Vector4 m_currentUp = new();

    public Vector4 m_currentVelocity = new();

    public Vector4 m_desiredVelocity = new();

    public Vector4 m_localGoalPlane = new();

    public float m_distToLocalGoal;

    public bool m_applyKinematicConstraints;

    public bool m_applyAvoidanceSteering;

    public bool m_enableLocalSteering;

}


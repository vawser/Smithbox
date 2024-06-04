// Automatically Generated

namespace HKLib.hk2018;

public class hkpWheelFrictionConstraintAtom : hkpConstraintAtom
{
    public byte m_isEnabled;

    public byte m_forwardAxis;

    public byte m_sideAxis;

    public float m_radius;

    public hkpWheelFrictionConstraintAtom.Axle? m_axle;

    public float m_maxFrictionForce;

    public float m_torque;

    public readonly float[] m_frictionImpulse = new float[2];

    public readonly float[] m_slipImpulse = new float[2];


    public class Axle : IHavokObject
    {
        public float m_spinVelocity;

        public float m_sumVelocity;

        public int m_numWheels;

        public int m_wheelsSolved;

        public int m_stepsSolved;

        public float m_invInertia;

        public float m_inertia;

        public float m_impulseScaling;

        public float m_impulseMax;

        public bool m_isFixed;

        public int m_numWheelsOnGround;

    }


}


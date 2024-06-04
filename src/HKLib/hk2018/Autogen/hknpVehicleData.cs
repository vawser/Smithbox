// Automatically Generated

namespace HKLib.hk2018;

public class hknpVehicleData : hkReferencedObject
{
    public Vector4 m_gravity = new();

    public sbyte m_numWheels;

    public Matrix3x3 m_chassisOrientation = new();

    public float m_torqueRollFactor;

    public float m_torquePitchFactor;

    public float m_torqueYawFactor;

    public float m_extraTorqueFactor;

    public float m_maxVelocityForPositionalFriction;

    public float m_chassisUnitInertiaYaw;

    public float m_chassisUnitInertiaRoll;

    public float m_chassisUnitInertiaPitch;

    public float m_frictionEqualizer;

    public float m_normalClippingAngleCos;

    public float m_maxFrictionSolverMassRatio;

    public List<hknpVehicleData.WheelComponentParams> m_wheelParams = new();


    public class WheelComponentParams : IHavokObject
    {
        public float m_radius;

        public float m_mass;

        public float m_width;

        public float m_friction;

        public float m_viscosityFriction;

        public float m_maxFriction;

        public float m_slipAngle;

        public float m_forceFeedbackMultiplier;

        public float m_maxContactBodyAcceleration;

        public sbyte m_axle;

    }


}


// Automatically Generated

namespace HKLib.hk2018;

public class hknpVehicleInstance : hknpUnaryAction
{
    public hknpVehicleData? m_data;

    public hknpVehicleDriverInput? m_driverInput;

    public hknpVehicleSteering? m_steering;

    public hknpVehicleEngine? m_engine;

    public hknpVehicleTransmission? m_transmission;

    public hknpVehicleBrake? m_brake;

    public hknpVehicleSuspension? m_suspension;

    public hknpVehicleAerodynamics? m_aerodynamics;

    public hknpVehicleWheelCollide? m_wheelCollide;

    public hknpTyremarksInfo? m_tyreMarks;

    public hknpVehicleVelocityDamper? m_velocityDamper;

    public List<hknpVehicleInstance.WheelInfo> m_wheelsInfo = new();

    public hkpVehicleFrictionStatus m_frictionStatus = new();

    public hknpVehicleDriverInputStatus? m_deviceStatus;

    public List<bool> m_isFixed = new();

    public float m_wheelsTimeSinceMaxPedalInput;

    public bool m_tryingToReverse;

    public float m_torque;

    public float m_rpm;

    public float m_mainSteeringAngle;

    public float m_mainSteeringAngleAssumingNoReduction;

    public List<float> m_wheelsSteeringAngle = new();

    public bool m_isReversing;

    public sbyte m_currentGear;

    public bool m_delayed;

    public float m_clutchDelayCountdown;


    public class WheelInfo : IHavokObject
    {
        public hkContactPoint m_contactPoint = new();

        public float m_contactFriction;

        public uint m_contactShapeKey;

        public Vector4 m_hardPointWs = new();

        public Vector4 m_rayEndPointWs = new();

        public float m_currentSuspensionLength;

        public Vector4 m_suspensionDirectionWs = new();

        public Vector4 m_spinAxisChassisSpace = new();

        public Vector4 m_spinAxisWs = new();

        public Quaternion m_steeringOrientationChassisSpace = new();

        public float m_spinVelocity;

        public float m_noSlipIdealSpinVelocity;

        public float m_spinAngle;

        public float m_skidEnergyDensity;

        public float m_sideForce;

        public float m_forwardSlipVelocity;

        public float m_sideSlipVelocity;

    }


}


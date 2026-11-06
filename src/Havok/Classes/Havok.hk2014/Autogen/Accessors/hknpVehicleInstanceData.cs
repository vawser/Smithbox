// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpVehicleInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpVehicleInstance>
{
    public hknpVehicleInstanceData(Havok.hk2014.hknpVehicleInstance instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_bodyId":
            case "bodyId":
            {
                value = instance.m_bodyId;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_driverInput":
            case "driverInput":
            {
                value = instance.m_driverInput;
                return true;
            }
            case "m_steering":
            case "steering":
            {
                value = instance.m_steering;
                return true;
            }
            case "m_engine":
            case "engine":
            {
                value = instance.m_engine;
                return true;
            }
            case "m_transmission":
            case "transmission":
            {
                value = instance.m_transmission;
                return true;
            }
            case "m_brake":
            case "brake":
            {
                value = instance.m_brake;
                return true;
            }
            case "m_suspension":
            case "suspension":
            {
                value = instance.m_suspension;
                return true;
            }
            case "m_aerodynamics":
            case "aerodynamics":
            {
                value = instance.m_aerodynamics;
                return true;
            }
            case "m_wheelCollide":
            case "wheelCollide":
            {
                value = instance.m_wheelCollide;
                return true;
            }
            case "m_tyreMarks":
            case "tyreMarks":
            {
                value = instance.m_tyreMarks;
                return true;
            }
            case "m_velocityDamper":
            case "velocityDamper":
            {
                value = instance.m_velocityDamper;
                return true;
            }
            case "m_wheelsInfo":
            case "wheelsInfo":
            {
                value = instance.m_wheelsInfo;
                return true;
            }
            case "m_frictionStatus":
            case "frictionStatus":
            {
                value = instance.m_frictionStatus;
                return true;
            }
            case "m_deviceStatus":
            case "deviceStatus":
            {
                value = instance.m_deviceStatus;
                return true;
            }
            case "m_isFixed":
            case "isFixed":
            {
                value = instance.m_isFixed;
                return true;
            }
            case "m_wheelsTimeSinceMaxPedalInput":
            case "wheelsTimeSinceMaxPedalInput":
            {
                value = instance.m_wheelsTimeSinceMaxPedalInput;
                return true;
            }
            case "m_tryingToReverse":
            case "tryingToReverse":
            {
                value = instance.m_tryingToReverse;
                return true;
            }
            case "m_torque":
            case "torque":
            {
                value = instance.m_torque;
                return true;
            }
            case "m_rpm":
            case "rpm":
            {
                value = instance.m_rpm;
                return true;
            }
            case "m_mainSteeringAngle":
            case "mainSteeringAngle":
            {
                value = instance.m_mainSteeringAngle;
                return true;
            }
            case "m_mainSteeringAngleAssumingNoReduction":
            case "mainSteeringAngleAssumingNoReduction":
            {
                value = instance.m_mainSteeringAngleAssumingNoReduction;
                return true;
            }
            case "m_wheelsSteeringAngle":
            case "wheelsSteeringAngle":
            {
                value = instance.m_wheelsSteeringAngle;
                return true;
            }
            case "m_isReversing":
            case "isReversing":
            {
                value = instance.m_isReversing;
                return true;
            }
            case "m_currentGear":
            case "currentGear":
            {
                value = instance.m_currentGear;
                return true;
            }
            case "m_delayed":
            case "delayed":
            {
                value = instance.m_delayed;
                return true;
            }
            case "m_clutchDelayCountdown":
            case "clutchDelayCountdown":
            {
                value = instance.m_clutchDelayCountdown;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_bodyId":
            case "bodyId":
            {
                if (value is not uint castValue) return false;
                instance.m_bodyId = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is null)
                {
                    instance.m_data = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpVehicleData castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            case "m_driverInput":
            case "driverInput":
            {
                if (value is null)
                {
                    instance.m_driverInput = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpVehicleDriverInput castValue)
                {
                    instance.m_driverInput = castValue;
                    return true;
                }
                return false;
            }
            case "m_steering":
            case "steering":
            {
                if (value is null)
                {
                    instance.m_steering = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpVehicleSteering castValue)
                {
                    instance.m_steering = castValue;
                    return true;
                }
                return false;
            }
            case "m_engine":
            case "engine":
            {
                if (value is null)
                {
                    instance.m_engine = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpVehicleEngine castValue)
                {
                    instance.m_engine = castValue;
                    return true;
                }
                return false;
            }
            case "m_transmission":
            case "transmission":
            {
                if (value is null)
                {
                    instance.m_transmission = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpVehicleTransmission castValue)
                {
                    instance.m_transmission = castValue;
                    return true;
                }
                return false;
            }
            case "m_brake":
            case "brake":
            {
                if (value is null)
                {
                    instance.m_brake = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpVehicleBrake castValue)
                {
                    instance.m_brake = castValue;
                    return true;
                }
                return false;
            }
            case "m_suspension":
            case "suspension":
            {
                if (value is null)
                {
                    instance.m_suspension = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpVehicleSuspension castValue)
                {
                    instance.m_suspension = castValue;
                    return true;
                }
                return false;
            }
            case "m_aerodynamics":
            case "aerodynamics":
            {
                if (value is null)
                {
                    instance.m_aerodynamics = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpVehicleAerodynamics castValue)
                {
                    instance.m_aerodynamics = castValue;
                    return true;
                }
                return false;
            }
            case "m_wheelCollide":
            case "wheelCollide":
            {
                if (value is null)
                {
                    instance.m_wheelCollide = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpVehicleWheelCollide castValue)
                {
                    instance.m_wheelCollide = castValue;
                    return true;
                }
                return false;
            }
            case "m_tyreMarks":
            case "tyreMarks":
            {
                if (value is null)
                {
                    instance.m_tyreMarks = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpTyremarksInfo castValue)
                {
                    instance.m_tyreMarks = castValue;
                    return true;
                }
                return false;
            }
            case "m_velocityDamper":
            case "velocityDamper":
            {
                if (value is null)
                {
                    instance.m_velocityDamper = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpVehicleVelocityDamper castValue)
                {
                    instance.m_velocityDamper = castValue;
                    return true;
                }
                return false;
            }
            case "m_wheelsInfo":
            case "wheelsInfo":
            {
                if (value is not List<Havok.hk2014.hknpVehicleInstance.WheelInfo> castValue) return false;
                instance.m_wheelsInfo = castValue;
                return true;
            }
            case "m_frictionStatus":
            case "frictionStatus":
            {
                if (value is not Havok.hk2014.hkpVehicleFrictionStatus castValue) return false;
                instance.m_frictionStatus = castValue;
                return true;
            }
            case "m_deviceStatus":
            case "deviceStatus":
            {
                if (value is null)
                {
                    instance.m_deviceStatus = default;
                    return true;
                }
                if (value is Havok.hk2014.hknpVehicleDriverInputStatus castValue)
                {
                    instance.m_deviceStatus = castValue;
                    return true;
                }
                return false;
            }
            case "m_isFixed":
            case "isFixed":
            {
                if (value is not List<bool> castValue) return false;
                instance.m_isFixed = castValue;
                return true;
            }
            case "m_wheelsTimeSinceMaxPedalInput":
            case "wheelsTimeSinceMaxPedalInput":
            {
                if (value is not float castValue) return false;
                instance.m_wheelsTimeSinceMaxPedalInput = castValue;
                return true;
            }
            case "m_tryingToReverse":
            case "tryingToReverse":
            {
                if (value is not bool castValue) return false;
                instance.m_tryingToReverse = castValue;
                return true;
            }
            case "m_torque":
            case "torque":
            {
                if (value is not float castValue) return false;
                instance.m_torque = castValue;
                return true;
            }
            case "m_rpm":
            case "rpm":
            {
                if (value is not float castValue) return false;
                instance.m_rpm = castValue;
                return true;
            }
            case "m_mainSteeringAngle":
            case "mainSteeringAngle":
            {
                if (value is not float castValue) return false;
                instance.m_mainSteeringAngle = castValue;
                return true;
            }
            case "m_mainSteeringAngleAssumingNoReduction":
            case "mainSteeringAngleAssumingNoReduction":
            {
                if (value is not float castValue) return false;
                instance.m_mainSteeringAngleAssumingNoReduction = castValue;
                return true;
            }
            case "m_wheelsSteeringAngle":
            case "wheelsSteeringAngle":
            {
                if (value is not List<float> castValue) return false;
                instance.m_wheelsSteeringAngle = castValue;
                return true;
            }
            case "m_isReversing":
            case "isReversing":
            {
                if (value is not bool castValue) return false;
                instance.m_isReversing = castValue;
                return true;
            }
            case "m_currentGear":
            case "currentGear":
            {
                if (value is not sbyte castValue) return false;
                instance.m_currentGear = castValue;
                return true;
            }
            case "m_delayed":
            case "delayed":
            {
                if (value is not bool castValue) return false;
                instance.m_delayed = castValue;
                return true;
            }
            case "m_clutchDelayCountdown":
            case "clutchDelayCountdown":
            {
                if (value is not float castValue) return false;
                instance.m_clutchDelayCountdown = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

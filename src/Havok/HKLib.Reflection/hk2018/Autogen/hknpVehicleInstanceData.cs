// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleInstanceData : HavokData<hknpVehicleInstance> 
{
    public hknpVehicleInstanceData(HavokType type, hknpVehicleInstance instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyId":
            case "bodyId":
            {
                if (instance.m_bodyId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is null)
                {
                    return true;
                }
                if (instance.m_data is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_driverInput":
            case "driverInput":
            {
                if (instance.m_driverInput is null)
                {
                    return true;
                }
                if (instance.m_driverInput is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_steering":
            case "steering":
            {
                if (instance.m_steering is null)
                {
                    return true;
                }
                if (instance.m_steering is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_engine":
            case "engine":
            {
                if (instance.m_engine is null)
                {
                    return true;
                }
                if (instance.m_engine is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_transmission":
            case "transmission":
            {
                if (instance.m_transmission is null)
                {
                    return true;
                }
                if (instance.m_transmission is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_brake":
            case "brake":
            {
                if (instance.m_brake is null)
                {
                    return true;
                }
                if (instance.m_brake is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_suspension":
            case "suspension":
            {
                if (instance.m_suspension is null)
                {
                    return true;
                }
                if (instance.m_suspension is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_aerodynamics":
            case "aerodynamics":
            {
                if (instance.m_aerodynamics is null)
                {
                    return true;
                }
                if (instance.m_aerodynamics is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_wheelCollide":
            case "wheelCollide":
            {
                if (instance.m_wheelCollide is null)
                {
                    return true;
                }
                if (instance.m_wheelCollide is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_tyreMarks":
            case "tyreMarks":
            {
                if (instance.m_tyreMarks is null)
                {
                    return true;
                }
                if (instance.m_tyreMarks is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_velocityDamper":
            case "velocityDamper":
            {
                if (instance.m_velocityDamper is null)
                {
                    return true;
                }
                if (instance.m_velocityDamper is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_wheelsInfo":
            case "wheelsInfo":
            {
                if (instance.m_wheelsInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_frictionStatus":
            case "frictionStatus":
            {
                if (instance.m_frictionStatus is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_deviceStatus":
            case "deviceStatus":
            {
                if (instance.m_deviceStatus is null)
                {
                    return true;
                }
                if (instance.m_deviceStatus is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_isFixed":
            case "isFixed":
            {
                if (instance.m_isFixed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelsTimeSinceMaxPedalInput":
            case "wheelsTimeSinceMaxPedalInput":
            {
                if (instance.m_wheelsTimeSinceMaxPedalInput is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tryingToReverse":
            case "tryingToReverse":
            {
                if (instance.m_tryingToReverse is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_torque":
            case "torque":
            {
                if (instance.m_torque is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rpm":
            case "rpm":
            {
                if (instance.m_rpm is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mainSteeringAngle":
            case "mainSteeringAngle":
            {
                if (instance.m_mainSteeringAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mainSteeringAngleAssumingNoReduction":
            case "mainSteeringAngleAssumingNoReduction":
            {
                if (instance.m_mainSteeringAngleAssumingNoReduction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelsSteeringAngle":
            case "wheelsSteeringAngle":
            {
                if (instance.m_wheelsSteeringAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isReversing":
            case "isReversing":
            {
                if (instance.m_isReversing is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentGear":
            case "currentGear":
            {
                if (instance.m_currentGear is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_delayed":
            case "delayed":
            {
                if (instance.m_delayed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clutchDelayCountdown":
            case "clutchDelayCountdown":
            {
                if (instance.m_clutchDelayCountdown is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
                if (value is not hknpBodyId castValue) return false;
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
                if (value is hknpVehicleData castValue)
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
                if (value is hknpVehicleDriverInput castValue)
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
                if (value is hknpVehicleSteering castValue)
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
                if (value is hknpVehicleEngine castValue)
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
                if (value is hknpVehicleTransmission castValue)
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
                if (value is hknpVehicleBrake castValue)
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
                if (value is hknpVehicleSuspension castValue)
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
                if (value is hknpVehicleAerodynamics castValue)
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
                if (value is hknpVehicleWheelCollide castValue)
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
                if (value is hknpTyremarksInfo castValue)
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
                if (value is hknpVehicleVelocityDamper castValue)
                {
                    instance.m_velocityDamper = castValue;
                    return true;
                }
                return false;
            }
            case "m_wheelsInfo":
            case "wheelsInfo":
            {
                if (value is not List<hknpVehicleInstance.WheelInfo> castValue) return false;
                instance.m_wheelsInfo = castValue;
                return true;
            }
            case "m_frictionStatus":
            case "frictionStatus":
            {
                if (value is not hkpVehicleFrictionStatus castValue) return false;
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
                if (value is hknpVehicleDriverInputStatus castValue)
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

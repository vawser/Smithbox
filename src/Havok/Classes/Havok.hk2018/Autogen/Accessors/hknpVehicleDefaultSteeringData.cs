// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpVehicleDefaultSteeringData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpVehicleDefaultSteering>
{
    public hknpVehicleDefaultSteeringData(Havok.hk2018.hknpVehicleDefaultSteering instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_maxSteeringAngle":
            case "maxSteeringAngle":
            {
                value = instance.m_maxSteeringAngle;
                return true;
            }
            case "m_maxSpeedFullSteeringAngle":
            case "maxSpeedFullSteeringAngle":
            {
                value = instance.m_maxSpeedFullSteeringAngle;
                return true;
            }
            case "m_doesWheelSteer":
            case "doesWheelSteer":
            {
                value = instance.m_doesWheelSteer;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_maxSteeringAngle":
            case "maxSteeringAngle":
            {
                if (value is not float castValue) return false;
                instance.m_maxSteeringAngle = castValue;
                return true;
            }
            case "m_maxSpeedFullSteeringAngle":
            case "maxSpeedFullSteeringAngle":
            {
                if (value is not float castValue) return false;
                instance.m_maxSpeedFullSteeringAngle = castValue;
                return true;
            }
            case "m_doesWheelSteer":
            case "doesWheelSteer":
            {
                if (value is not List<bool> castValue) return false;
                instance.m_doesWheelSteer = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleDefaultSteeringData : HavokData<hknpVehicleDefaultSteering> 
{
    public hknpVehicleDefaultSteeringData(HavokType type, hknpVehicleDefaultSteering instance) : base(type, instance) {}

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
            case "m_maxSteeringAngle":
            case "maxSteeringAngle":
            {
                if (instance.m_maxSteeringAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxSpeedFullSteeringAngle":
            case "maxSpeedFullSteeringAngle":
            {
                if (instance.m_maxSpeedFullSteeringAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_doesWheelSteer":
            case "doesWheelSteer":
            {
                if (instance.m_doesWheelSteer is not TGet castValue) return false;
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

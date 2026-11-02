// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpVehicleDefaultSteeringData : HavokData<HKLib.hk2019.hknpVehicleDefaultSteering> 
{
    public hknpVehicleDefaultSteeringData(HavokType type, HKLib.hk2019.hknpVehicleDefaultSteering instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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

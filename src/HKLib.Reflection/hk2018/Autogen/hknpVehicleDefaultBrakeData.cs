// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleDefaultBrakeData : HavokData<hknpVehicleDefaultBrake> 
{
    public hknpVehicleDefaultBrakeData(HavokType type, hknpVehicleDefaultBrake instance) : base(type, instance) {}

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
            case "m_wheelBrakingProperties":
            case "wheelBrakingProperties":
            {
                if (instance.m_wheelBrakingProperties is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelsMinTimeToBlock":
            case "wheelsMinTimeToBlock":
            {
                if (instance.m_wheelsMinTimeToBlock is not TGet castValue) return false;
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
            case "m_wheelBrakingProperties":
            case "wheelBrakingProperties":
            {
                if (value is not List<hknpVehicleDefaultBrake.WheelBrakingProperties> castValue) return false;
                instance.m_wheelBrakingProperties = castValue;
                return true;
            }
            case "m_wheelsMinTimeToBlock":
            case "wheelsMinTimeToBlock":
            {
                if (value is not float castValue) return false;
                instance.m_wheelsMinTimeToBlock = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

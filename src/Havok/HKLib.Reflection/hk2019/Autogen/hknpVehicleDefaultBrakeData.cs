// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpVehicleDefaultBrakeData : HavokData<HKLib.hk2019.hknpVehicleDefaultBrake> 
{
    public hknpVehicleDefaultBrakeData(HavokType type, HKLib.hk2019.hknpVehicleDefaultBrake instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_wheelBrakingProperties":
            case "wheelBrakingProperties":
            {
                if (value is not List<HKLib.hk2019.hknpVehicleDefaultBrake.WheelBrakingProperties> castValue) return false;
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

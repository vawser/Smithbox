// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpVehicleDefaultBrakeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpVehicleDefaultBrake>
{
    public hknpVehicleDefaultBrakeData(Havok.hk2014.hknpVehicleDefaultBrake instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_wheelBrakingProperties":
            case "wheelBrakingProperties":
            {
                value = instance.m_wheelBrakingProperties;
                return true;
            }
            case "m_wheelsMinTimeToBlock":
            case "wheelsMinTimeToBlock":
            {
                value = instance.m_wheelsMinTimeToBlock;
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
            case "m_wheelBrakingProperties":
            case "wheelBrakingProperties":
            {
                if (value is not List<Havok.hk2014.hknpVehicleDefaultBrake.WheelBrakingProperties> castValue) return false;
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

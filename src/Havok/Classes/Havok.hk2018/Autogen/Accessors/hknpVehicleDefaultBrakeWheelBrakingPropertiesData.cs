// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpVehicleDefaultBrakeWheelBrakingPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpVehicleDefaultBrake.WheelBrakingProperties>
{
    public hknpVehicleDefaultBrakeWheelBrakingPropertiesData(Havok.hk2018.hknpVehicleDefaultBrake.WheelBrakingProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxBreakingTorque":
            case "maxBreakingTorque":
            {
                value = instance.m_maxBreakingTorque;
                return true;
            }
            case "m_minPedalInputToBlock":
            case "minPedalInputToBlock":
            {
                value = instance.m_minPedalInputToBlock;
                return true;
            }
            case "m_isConnectedToHandbrake":
            case "isConnectedToHandbrake":
            {
                value = instance.m_isConnectedToHandbrake;
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
            case "m_maxBreakingTorque":
            case "maxBreakingTorque":
            {
                if (value is not float castValue) return false;
                instance.m_maxBreakingTorque = castValue;
                return true;
            }
            case "m_minPedalInputToBlock":
            case "minPedalInputToBlock":
            {
                if (value is not float castValue) return false;
                instance.m_minPedalInputToBlock = castValue;
                return true;
            }
            case "m_isConnectedToHandbrake":
            case "isConnectedToHandbrake":
            {
                if (value is not bool castValue) return false;
                instance.m_isConnectedToHandbrake = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleDefaultBrakeWheelBrakingPropertiesData : HavokData<hknpVehicleDefaultBrake.WheelBrakingProperties> 
{
    public hknpVehicleDefaultBrakeWheelBrakingPropertiesData(HavokType type, hknpVehicleDefaultBrake.WheelBrakingProperties instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxBreakingTorque":
            case "maxBreakingTorque":
            {
                if (instance.m_maxBreakingTorque is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minPedalInputToBlock":
            case "minPedalInputToBlock":
            {
                if (instance.m_minPedalInputToBlock is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isConnectedToHandbrake":
            case "isConnectedToHandbrake":
            {
                if (instance.m_isConnectedToHandbrake is not TGet castValue) return false;
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

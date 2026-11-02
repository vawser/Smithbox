// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpVehicleSuspensionSuspensionWheelParametersData : HavokData<HKLib.hk2019.hknpVehicleSuspension.SuspensionWheelParameters> 
{
    public hknpVehicleSuspensionSuspensionWheelParametersData(HavokType type, HKLib.hk2019.hknpVehicleSuspension.SuspensionWheelParameters instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_hardpointChassisSpace":
            case "hardpointChassisSpace":
            {
                if (instance.m_hardpointChassisSpace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_directionChassisSpace":
            case "directionChassisSpace":
            {
                if (instance.m_directionChassisSpace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_length":
            case "length":
            {
                if (instance.m_length is not TGet castValue) return false;
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
            case "m_hardpointChassisSpace":
            case "hardpointChassisSpace":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_hardpointChassisSpace = castValue;
                return true;
            }
            case "m_directionChassisSpace":
            case "directionChassisSpace":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_directionChassisSpace = castValue;
                return true;
            }
            case "m_length":
            case "length":
            {
                if (value is not float castValue) return false;
                instance.m_length = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpVehicleSuspensionSuspensionWheelParametersData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpVehicleSuspension.SuspensionWheelParameters>
{
    public hknpVehicleSuspensionSuspensionWheelParametersData(Havok.hk2019.hknpVehicleSuspension.SuspensionWheelParameters instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_hardpointChassisSpace":
            case "hardpointChassisSpace":
            {
                value = instance.m_hardpointChassisSpace;
                return true;
            }
            case "m_directionChassisSpace":
            case "directionChassisSpace":
            {
                value = instance.m_directionChassisSpace;
                return true;
            }
            case "m_length":
            case "length":
            {
                value = instance.m_length;
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

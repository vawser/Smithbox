// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpVehicleDriverInputAnalogStatusData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpVehicleDriverInputAnalogStatus>
{
    public hknpVehicleDriverInputAnalogStatusData(Havok.hk2014.hknpVehicleDriverInputAnalogStatus instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_positionX":
            case "positionX":
            {
                value = instance.m_positionX;
                return true;
            }
            case "m_positionY":
            case "positionY":
            {
                value = instance.m_positionY;
                return true;
            }
            case "m_handbrakeButtonPressed":
            case "handbrakeButtonPressed":
            {
                value = instance.m_handbrakeButtonPressed;
                return true;
            }
            case "m_reverseButtonPressed":
            case "reverseButtonPressed":
            {
                value = instance.m_reverseButtonPressed;
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
            case "m_positionX":
            case "positionX":
            {
                if (value is not float castValue) return false;
                instance.m_positionX = castValue;
                return true;
            }
            case "m_positionY":
            case "positionY":
            {
                if (value is not float castValue) return false;
                instance.m_positionY = castValue;
                return true;
            }
            case "m_handbrakeButtonPressed":
            case "handbrakeButtonPressed":
            {
                if (value is not bool castValue) return false;
                instance.m_handbrakeButtonPressed = castValue;
                return true;
            }
            case "m_reverseButtonPressed":
            case "reverseButtonPressed":
            {
                if (value is not bool castValue) return false;
                instance.m_reverseButtonPressed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

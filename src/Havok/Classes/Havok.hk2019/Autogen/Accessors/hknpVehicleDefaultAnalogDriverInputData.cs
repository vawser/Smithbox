// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpVehicleDefaultAnalogDriverInputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpVehicleDefaultAnalogDriverInput>
{
    public hknpVehicleDefaultAnalogDriverInputData(Havok.hk2019.hknpVehicleDefaultAnalogDriverInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_slopeChangePointX":
            case "slopeChangePointX":
            {
                value = instance.m_slopeChangePointX;
                return true;
            }
            case "m_initialSlope":
            case "initialSlope":
            {
                value = instance.m_initialSlope;
                return true;
            }
            case "m_deadZone":
            case "deadZone":
            {
                value = instance.m_deadZone;
                return true;
            }
            case "m_autoReverse":
            case "autoReverse":
            {
                value = instance.m_autoReverse;
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
            case "m_slopeChangePointX":
            case "slopeChangePointX":
            {
                if (value is not float castValue) return false;
                instance.m_slopeChangePointX = castValue;
                return true;
            }
            case "m_initialSlope":
            case "initialSlope":
            {
                if (value is not float castValue) return false;
                instance.m_initialSlope = castValue;
                return true;
            }
            case "m_deadZone":
            case "deadZone":
            {
                if (value is not float castValue) return false;
                instance.m_deadZone = castValue;
                return true;
            }
            case "m_autoReverse":
            case "autoReverse":
            {
                if (value is not bool castValue) return false;
                instance.m_autoReverse = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

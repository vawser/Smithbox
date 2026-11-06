// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpVehicleDefaultSuspensionWheelSpringSuspensionParametersData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpVehicleDefaultSuspension.WheelSpringSuspensionParameters>
{
    public hknpVehicleDefaultSuspensionWheelSpringSuspensionParametersData(Havok.hk2014.hknpVehicleDefaultSuspension.WheelSpringSuspensionParameters instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_strength":
            case "strength":
            {
                value = instance.m_strength;
                return true;
            }
            case "m_dampingCompression":
            case "dampingCompression":
            {
                value = instance.m_dampingCompression;
                return true;
            }
            case "m_dampingRelaxation":
            case "dampingRelaxation":
            {
                value = instance.m_dampingRelaxation;
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
            case "m_strength":
            case "strength":
            {
                if (value is not float castValue) return false;
                instance.m_strength = castValue;
                return true;
            }
            case "m_dampingCompression":
            case "dampingCompression":
            {
                if (value is not float castValue) return false;
                instance.m_dampingCompression = castValue;
                return true;
            }
            case "m_dampingRelaxation":
            case "dampingRelaxation":
            {
                if (value is not float castValue) return false;
                instance.m_dampingRelaxation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

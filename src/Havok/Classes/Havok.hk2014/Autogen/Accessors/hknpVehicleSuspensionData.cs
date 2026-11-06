// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpVehicleSuspensionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpVehicleSuspension>
{
    public hknpVehicleSuspensionData(Havok.hk2014.hknpVehicleSuspension instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_wheelParams":
            case "wheelParams":
            {
                value = instance.m_wheelParams;
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
            case "m_wheelParams":
            case "wheelParams":
            {
                if (value is not List<Havok.hk2014.hknpVehicleSuspension.SuspensionWheelParameters> castValue) return false;
                instance.m_wheelParams = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

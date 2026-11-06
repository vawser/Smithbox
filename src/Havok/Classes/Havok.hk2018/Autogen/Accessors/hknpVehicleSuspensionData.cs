// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpVehicleSuspensionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpVehicleSuspension>
{
    public hknpVehicleSuspensionData(Havok.hk2018.hknpVehicleSuspension instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_wheelParams":
            case "wheelParams":
            {
                if (value is not List<Havok.hk2018.hknpVehicleSuspension.SuspensionWheelParameters> castValue) return false;
                instance.m_wheelParams = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

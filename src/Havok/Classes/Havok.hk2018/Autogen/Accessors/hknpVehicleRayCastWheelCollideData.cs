// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpVehicleRayCastWheelCollideData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpVehicleRayCastWheelCollide>
{
    public hknpVehicleRayCastWheelCollideData(Havok.hk2018.hknpVehicleRayCastWheelCollide instance) : base(instance) { }

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
            case "m_alreadyUsed":
            case "alreadyUsed":
            {
                value = instance.m_alreadyUsed;
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
            case "m_alreadyUsed":
            case "alreadyUsed":
            {
                if (value is not bool castValue) return false;
                instance.m_alreadyUsed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

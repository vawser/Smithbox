// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpVehicleWheelCollideData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpVehicleWheelCollide>
{
    public hknpVehicleWheelCollideData(Havok.hk2014.hknpVehicleWheelCollide instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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

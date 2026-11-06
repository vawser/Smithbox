// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpVehicleDefaultVelocityDamperData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpVehicleDefaultVelocityDamper>
{
    public hknpVehicleDefaultVelocityDamperData(Havok.hk2014.hknpVehicleDefaultVelocityDamper instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_normalSpinDamping":
            case "normalSpinDamping":
            {
                value = instance.m_normalSpinDamping;
                return true;
            }
            case "m_collisionSpinDamping":
            case "collisionSpinDamping":
            {
                value = instance.m_collisionSpinDamping;
                return true;
            }
            case "m_collisionThreshold":
            case "collisionThreshold":
            {
                value = instance.m_collisionThreshold;
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
            case "m_normalSpinDamping":
            case "normalSpinDamping":
            {
                if (value is not float castValue) return false;
                instance.m_normalSpinDamping = castValue;
                return true;
            }
            case "m_collisionSpinDamping":
            case "collisionSpinDamping":
            {
                if (value is not float castValue) return false;
                instance.m_collisionSpinDamping = castValue;
                return true;
            }
            case "m_collisionThreshold":
            case "collisionThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_collisionThreshold = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

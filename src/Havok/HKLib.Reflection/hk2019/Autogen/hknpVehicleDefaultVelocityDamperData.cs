// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpVehicleDefaultVelocityDamperData : HavokData<HKLib.hk2019.hknpVehicleDefaultVelocityDamper> 
{
    public hknpVehicleDefaultVelocityDamperData(HavokType type, HKLib.hk2019.hknpVehicleDefaultVelocityDamper instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_normalSpinDamping":
            case "normalSpinDamping":
            {
                if (instance.m_normalSpinDamping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionSpinDamping":
            case "collisionSpinDamping":
            {
                if (instance.m_collisionSpinDamping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionThreshold":
            case "collisionThreshold":
            {
                if (instance.m_collisionThreshold is not TGet castValue) return false;
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

// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleDefaultVelocityDamperData : HavokData<hknpVehicleDefaultVelocityDamper> 
{
    public hknpVehicleDefaultVelocityDamperData(HavokType type, hknpVehicleDefaultVelocityDamper instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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

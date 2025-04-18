// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleDefaultSuspensionData : HavokData<hknpVehicleDefaultSuspension> 
{
    public hknpVehicleDefaultSuspensionData(HavokType type, hknpVehicleDefaultSuspension instance) : base(type, instance) {}

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
            case "m_wheelParams":
            case "wheelParams":
            {
                if (instance.m_wheelParams is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelSpringParams":
            case "wheelSpringParams":
            {
                if (instance.m_wheelSpringParams is not TGet castValue) return false;
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
            case "m_wheelParams":
            case "wheelParams":
            {
                if (value is not List<hknpVehicleSuspension.SuspensionWheelParameters> castValue) return false;
                instance.m_wheelParams = castValue;
                return true;
            }
            case "m_wheelSpringParams":
            case "wheelSpringParams":
            {
                if (value is not List<hknpVehicleDefaultSuspension.WheelSpringSuspensionParameters> castValue) return false;
                instance.m_wheelSpringParams = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

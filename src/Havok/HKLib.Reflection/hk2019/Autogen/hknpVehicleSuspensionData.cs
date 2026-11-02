// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpVehicleSuspensionData : HavokData<HKLib.hk2019.hknpVehicleSuspension> 
{
    public hknpVehicleSuspensionData(HavokType type, HKLib.hk2019.hknpVehicleSuspension instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_wheelParams":
            case "wheelParams":
            {
                if (instance.m_wheelParams is not TGet castValue) return false;
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
            case "m_wheelParams":
            case "wheelParams":
            {
                if (value is not List<HKLib.hk2019.hknpVehicleSuspension.SuspensionWheelParameters> castValue) return false;
                instance.m_wheelParams = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

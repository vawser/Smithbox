// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpVehicleWheelCollideData : HavokData<HKLib.hk2019.hknpVehicleWheelCollide> 
{
    public hknpVehicleWheelCollideData(HavokType type, HKLib.hk2019.hknpVehicleWheelCollide instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_alreadyUsed":
            case "alreadyUsed":
            {
                if (instance.m_alreadyUsed is not TGet castValue) return false;
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

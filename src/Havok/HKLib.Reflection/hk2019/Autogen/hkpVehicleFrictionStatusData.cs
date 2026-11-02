// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpVehicleFrictionStatusData : HavokData<HKLib.hk2019.hkpVehicleFrictionStatus> 
{
    private static readonly System.Reflection.FieldInfo _axisInfo = typeof(HKLib.hk2019.hkpVehicleFrictionStatus).GetField("m_axis")!;
    public hkpVehicleFrictionStatusData(HavokType type, HKLib.hk2019.hkpVehicleFrictionStatus instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_axis":
            case "axis":
            {
                if (instance.m_axis is not TGet castValue) return false;
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
            case "m_axis":
            case "axis":
            {
                if (value is not HKLib.hk2019.hkpVehicleFrictionStatus.AxisStatus[] castValue || castValue.Length != 2) return false;
                try
                {
                    _axisInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}

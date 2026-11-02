// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkmsAccumulateData : HavokData<HKLib.hk2019.hkms.Accumulate> 
{
    public hkmsAccumulateData(HavokType type, HKLib.hk2019.hkms.Accumulate instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_value":
            case "value":
            {
                if (instance.m_value is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_value is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_value":
            case "value":
            {
                if (value is HKLib.hk2019.hkms.AccumulateOp.Enum castValue)
                {
                    instance.m_value = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_value = (HKLib.hk2019.hkms.AccumulateOp.Enum)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

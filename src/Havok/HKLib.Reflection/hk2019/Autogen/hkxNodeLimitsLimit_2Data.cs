// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxNodeLimitsLimit_2Data : HavokData<HKLib.hk2019.hkxNodeLimits.Limit_2> 
{
    private static readonly System.Reflection.FieldInfo _valuesInfo = typeof(HKLib.hk2019.hkxNodeLimits.Limit_2).GetField("m_values")!;
    public hkxNodeLimitsLimit_2Data(HavokType type, HKLib.hk2019.hkxNodeLimits.Limit_2 instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_values":
            case "values":
            {
                if (instance.m_values is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_active":
            case "active":
            {
                if (instance.m_active is not TGet castValue) return false;
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
            case "m_values":
            case "values":
            {
                if (value is not float[] castValue || castValue.Length != 6) return false;
                try
                {
                    _valuesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_active":
            case "active":
            {
                if (value is not byte castValue) return false;
                instance.m_active = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

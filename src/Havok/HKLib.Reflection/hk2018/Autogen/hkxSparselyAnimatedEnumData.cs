// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxSparselyAnimatedEnumData : HavokData<hkxSparselyAnimatedEnum> 
{
    public hkxSparselyAnimatedEnumData(HavokType type, hkxSparselyAnimatedEnum instance) : base(type, instance) {}

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
            case "m_ints":
            case "ints":
            {
                if (instance.m_ints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_times":
            case "times":
            {
                if (instance.m_times is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enum":
            case "enum":
            {
                if (instance.m_enum is null)
                {
                    return true;
                }
                if (instance.m_enum is TGet castValue)
                {
                    value = castValue;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_ints":
            case "ints":
            {
                if (value is not List<int> castValue) return false;
                instance.m_ints = castValue;
                return true;
            }
            case "m_times":
            case "times":
            {
                if (value is not List<float> castValue) return false;
                instance.m_times = castValue;
                return true;
            }
            case "m_enum":
            case "enum":
            {
                if (value is null)
                {
                    instance.m_enum = default;
                    return true;
                }
                if (value is hkxEnum castValue)
                {
                    instance.m_enum = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

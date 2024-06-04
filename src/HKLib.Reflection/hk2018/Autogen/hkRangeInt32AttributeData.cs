// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkRangeInt32AttributeData : HavokData<hkRangeInt32Attribute> 
{
    public hkRangeInt32AttributeData(HavokType type, hkRangeInt32Attribute instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_absmin":
            case "absmin":
            {
                if (instance.m_absmin is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_absmax":
            case "absmax":
            {
                if (instance.m_absmax is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_softmin":
            case "softmin":
            {
                if (instance.m_softmin is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_softmax":
            case "softmax":
            {
                if (instance.m_softmax is not TGet castValue) return false;
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
            case "m_absmin":
            case "absmin":
            {
                if (value is not int castValue) return false;
                instance.m_absmin = castValue;
                return true;
            }
            case "m_absmax":
            case "absmax":
            {
                if (value is not int castValue) return false;
                instance.m_absmax = castValue;
                return true;
            }
            case "m_softmin":
            case "softmin":
            {
                if (value is not int castValue) return false;
                instance.m_softmin = castValue;
                return true;
            }
            case "m_softmax":
            case "softmax":
            {
                if (value is not int castValue) return false;
                instance.m_softmax = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

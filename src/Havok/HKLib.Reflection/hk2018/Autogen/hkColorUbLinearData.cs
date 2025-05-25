// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkColorUbLinearData : HavokData<hkColorUbLinear> 
{
    public hkColorUbLinearData(HavokType type, hkColorUbLinear instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_r":
            case "r":
            {
                if (instance.m_r is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_g":
            case "g":
            {
                if (instance.m_g is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_b":
            case "b":
            {
                if (instance.m_b is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_a":
            case "a":
            {
                if (instance.m_a is not TGet castValue) return false;
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
            case "m_r":
            case "r":
            {
                if (value is not byte castValue) return false;
                instance.m_r = castValue;
                return true;
            }
            case "m_g":
            case "g":
            {
                if (value is not byte castValue) return false;
                instance.m_g = castValue;
                return true;
            }
            case "m_b":
            case "b":
            {
                if (value is not byte castValue) return false;
                instance.m_b = castValue;
                return true;
            }
            case "m_a":
            case "a":
            {
                if (value is not byte castValue) return false;
                instance.m_a = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

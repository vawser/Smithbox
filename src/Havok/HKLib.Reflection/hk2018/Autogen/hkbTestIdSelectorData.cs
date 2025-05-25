// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbTestIdSelectorData : HavokData<hkbTestIdSelector> 
{
    public hkbTestIdSelectorData(HavokType type, hkbTestIdSelector instance) : base(type, instance) {}

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
            case "m_int":
            case "int":
            {
                if (instance.m_int is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_real":
            case "real":
            {
                if (instance.m_real is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_string":
            case "string":
            {
                if (instance.m_string is null)
                {
                    return true;
                }
                if (instance.m_string is TGet castValue)
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
            case "m_int":
            case "int":
            {
                if (value is not int castValue) return false;
                instance.m_int = castValue;
                return true;
            }
            case "m_real":
            case "real":
            {
                if (value is not float castValue) return false;
                instance.m_real = castValue;
                return true;
            }
            case "m_string":
            case "string":
            {
                if (value is null)
                {
                    instance.m_string = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_string = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

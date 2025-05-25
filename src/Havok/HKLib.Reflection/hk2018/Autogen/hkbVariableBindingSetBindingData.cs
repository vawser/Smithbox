// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbVariableBindingSetBindingData : HavokData<hkbVariableBindingSet.Binding> 
{
    public hkbVariableBindingSetBindingData(HavokType type, hkbVariableBindingSet.Binding instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_memberPath":
            case "memberPath":
            {
                if (instance.m_memberPath is null)
                {
                    return true;
                }
                if (instance.m_memberPath is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_variableIndex":
            case "variableIndex":
            {
                if (instance.m_variableIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bitIndex":
            case "bitIndex":
            {
                if (instance.m_bitIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bindingType":
            case "bindingType":
            {
                if (instance.m_bindingType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_bindingType is TGet sbyteValue)
                {
                    value = sbyteValue;
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
            case "m_memberPath":
            case "memberPath":
            {
                if (value is null)
                {
                    instance.m_memberPath = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_memberPath = castValue;
                    return true;
                }
                return false;
            }
            case "m_variableIndex":
            case "variableIndex":
            {
                if (value is not int castValue) return false;
                instance.m_variableIndex = castValue;
                return true;
            }
            case "m_bitIndex":
            case "bitIndex":
            {
                if (value is not sbyte castValue) return false;
                instance.m_bitIndex = castValue;
                return true;
            }
            case "m_bindingType":
            case "bindingType":
            {
                if (value is hkbVariableBindingSet.Binding.BindingType castValue)
                {
                    instance.m_bindingType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_bindingType = (hkbVariableBindingSet.Binding.BindingType)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

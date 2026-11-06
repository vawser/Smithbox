// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbVariableBindingSetBindingData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbVariableBindingSet.Binding>
{
    public hkbVariableBindingSetBindingData(Havok.hk2018.hkbVariableBindingSet.Binding instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_memberPath":
            case "memberPath":
            {
                value = instance.m_memberPath;
                return true;
            }
            case "m_variableIndex":
            case "variableIndex":
            {
                value = instance.m_variableIndex;
                return true;
            }
            case "m_bitIndex":
            case "bitIndex":
            {
                value = instance.m_bitIndex;
                return true;
            }
            case "m_bindingType":
            case "bindingType":
            {
                value = instance.m_bindingType;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
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
                if (value is Havok.hk2018.hkbVariableBindingSet.Binding.BindingType castValue)
                {
                    instance.m_bindingType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_bindingType = (Havok.hk2018.hkbVariableBindingSet.Binding.BindingType)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

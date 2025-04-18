// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbVariableBindingSetData : HavokData<hkbVariableBindingSet> 
{
    public hkbVariableBindingSetData(HavokType type, hkbVariableBindingSet instance) : base(type, instance) {}

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
            case "m_bindings":
            case "bindings":
            {
                if (instance.m_bindings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_indexOfBindingToEnable":
            case "indexOfBindingToEnable":
            {
                if (instance.m_indexOfBindingToEnable is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_bindings":
            case "bindings":
            {
                if (value is not List<hkbVariableBindingSet.Binding> castValue) return false;
                instance.m_bindings = castValue;
                return true;
            }
            case "m_indexOfBindingToEnable":
            case "indexOfBindingToEnable":
            {
                if (value is not int castValue) return false;
                instance.m_indexOfBindingToEnable = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

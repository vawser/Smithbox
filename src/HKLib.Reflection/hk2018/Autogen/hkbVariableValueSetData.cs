// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbVariableValueSetData : HavokData<hkbVariableValueSet> 
{
    public hkbVariableValueSetData(HavokType type, hkbVariableValueSet instance) : base(type, instance) {}

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
            case "m_wordVariableValues":
            case "wordVariableValues":
            {
                if (instance.m_wordVariableValues is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_quadVariableValues":
            case "quadVariableValues":
            {
                if (instance.m_quadVariableValues is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variantVariableValues":
            case "variantVariableValues":
            {
                if (instance.m_variantVariableValues is not TGet castValue) return false;
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
            case "m_wordVariableValues":
            case "wordVariableValues":
            {
                if (value is not List<hkbVariableValue> castValue) return false;
                instance.m_wordVariableValues = castValue;
                return true;
            }
            case "m_quadVariableValues":
            case "quadVariableValues":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_quadVariableValues = castValue;
                return true;
            }
            case "m_variantVariableValues":
            case "variantVariableValues":
            {
                if (value is not List<hkReferencedObject?> castValue) return false;
                instance.m_variantVariableValues = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

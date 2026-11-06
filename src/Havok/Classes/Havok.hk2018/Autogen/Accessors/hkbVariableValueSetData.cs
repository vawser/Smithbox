// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbVariableValueSetData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbVariableValueSet>
{
    public hkbVariableValueSetData(Havok.hk2018.hkbVariableValueSet instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_wordVariableValues":
            case "wordVariableValues":
            {
                value = instance.m_wordVariableValues;
                return true;
            }
            case "m_quadVariableValues":
            case "quadVariableValues":
            {
                value = instance.m_quadVariableValues;
                return true;
            }
            case "m_variantVariableValues":
            case "variantVariableValues":
            {
                value = instance.m_variantVariableValues;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_wordVariableValues":
            case "wordVariableValues":
            {
                if (value is not List<Havok.hk2018.hkbVariableValue> castValue) return false;
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
                if (value is not List<Havok.hk2018.hkReferencedObject?> castValue) return false;
                instance.m_variantVariableValues = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

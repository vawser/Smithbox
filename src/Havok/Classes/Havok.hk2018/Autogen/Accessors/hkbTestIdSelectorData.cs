// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbTestIdSelectorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbTestIdSelector>
{
    public hkbTestIdSelectorData(Havok.hk2018.hkbTestIdSelector instance) : base(instance) { }

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
            case "m_int":
            case "int":
            {
                value = instance.m_int;
                return true;
            }
            case "m_real":
            case "real":
            {
                value = instance.m_real;
                return true;
            }
            case "m_string":
            case "string":
            {
                value = instance.m_string;
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

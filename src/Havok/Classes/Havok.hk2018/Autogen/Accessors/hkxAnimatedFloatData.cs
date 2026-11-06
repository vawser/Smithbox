// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxAnimatedFloatData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxAnimatedFloat>
{
    public hkxAnimatedFloatData(Havok.hk2018.hkxAnimatedFloat instance) : base(instance) { }

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
            case "m_floats":
            case "floats":
            {
                value = instance.m_floats;
                return true;
            }
            case "m_hint":
            case "hint":
            {
                value = instance.m_hint;
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
            case "m_floats":
            case "floats":
            {
                if (value is not List<float> castValue) return false;
                instance.m_floats = castValue;
                return true;
            }
            case "m_hint":
            case "hint":
            {
                if (value is Havok.hk2018.hkxAttribute.Hint castValue)
                {
                    instance.m_hint = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_hint = (Havok.hk2018.hkxAttribute.Hint)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

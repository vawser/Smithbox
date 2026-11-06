// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkColorUbLinearData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkColorUbLinear>
{
    public hkColorUbLinearData(Havok.hk2018.hkColorUbLinear instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_r":
            case "r":
            {
                value = instance.m_r;
                return true;
            }
            case "m_g":
            case "g":
            {
                value = instance.m_g;
                return true;
            }
            case "m_b":
            case "b":
            {
                value = instance.m_b;
                return true;
            }
            case "m_a":
            case "a":
            {
                value = instance.m_a;
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

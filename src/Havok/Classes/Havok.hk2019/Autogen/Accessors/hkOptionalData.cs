// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkOptionalData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hk.Optional>
{
    public hkOptionalData(Havok.hk2019.hk.Optional instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_optional":
            case "optional":
            {
                value = instance.m_optional;
                return true;
            }
            case "m_implicit":
            case "implicit":
            {
                value = instance.m_implicit;
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
            case "m_optional":
            case "optional":
            {
                if (value is not bool castValue) return false;
                instance.m_optional = castValue;
                return true;
            }
            case "m_implicit":
            case "implicit":
            {
                if (value is not bool castValue) return false;
                instance.m_implicit = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

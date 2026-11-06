// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkPropertyDescData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkPropertyDesc>
{
    public hkPropertyDescData(Havok.hk2018.hkPropertyDesc instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
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
            case "m_type":
            case "type":
            {
                if (value is null)
                {
                    instance.m_type = default;
                    return true;
                }
                if (value is Havok.hk2018.IHavokObject castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                return false;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2018.hkPropertyFlags.Enum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_flags = (Havok.hk2018.hkPropertyFlags.Enum)uintValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

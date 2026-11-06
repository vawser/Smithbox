// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkClassEnumData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkClassEnum>
{
    public hkClassEnumData(Havok.hk2014.hkClassEnum instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_items":
            case "items":
            {
                value = instance.m_items;
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
            case "m_items":
            case "items":
            {
                if (value is not List<Havok.hk2014.hkClassEnumItem> castValue) return false;
                instance.m_items = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2014.hkClassEnum.FlagValues castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_flags = (Havok.hk2014.hkClassEnum.FlagValues)uintValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

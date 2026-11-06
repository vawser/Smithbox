// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkClassMemberData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkClassMember>
{
    public hkClassMemberData(Havok.hk2014.hkClassMember instance) : base(instance) { }

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
            case "m_class":
            case "class":
            {
                value = instance.m_class;
                return true;
            }
            case "m_enum":
            case "enum":
            {
                value = instance.m_enum;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_subtype":
            case "subtype":
            {
                value = instance.m_subtype;
                return true;
            }
            case "m_cArraySize":
            case "cArraySize":
            {
                value = instance.m_cArraySize;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                value = instance.m_offset;
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
            case "m_class":
            case "class":
            {
                if (value is null)
                {
                    instance.m_class = default;
                    return true;
                }
                if (value is Havok.hk2014.hkClass castValue)
                {
                    instance.m_class = castValue;
                    return true;
                }
                return false;
            }
            case "m_enum":
            case "enum":
            {
                if (value is null)
                {
                    instance.m_enum = default;
                    return true;
                }
                if (value is Havok.hk2014.hkClassEnum castValue)
                {
                    instance.m_enum = castValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2014.hkClassMember.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2014.hkClassMember.Type)byteValue;
                    return true;
                }
                return false;
            }
            case "m_subtype":
            case "subtype":
            {
                if (value is Havok.hk2014.hkClassMember.Type castValue)
                {
                    instance.m_subtype = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_subtype = (Havok.hk2014.hkClassMember.Type)byteValue;
                    return true;
                }
                return false;
            }
            case "m_cArraySize":
            case "cArraySize":
            {
                if (value is not short castValue) return false;
                instance.m_cArraySize = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2014.hkClassMember.FlagValues castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (Havok.hk2014.hkClassMember.FlagValues)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_offset":
            case "offset":
            {
                if (value is not ushort castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

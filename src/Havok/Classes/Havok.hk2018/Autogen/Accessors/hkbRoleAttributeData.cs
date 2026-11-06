// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbRoleAttributeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbRoleAttribute>
{
    public hkbRoleAttributeData(Havok.hk2018.hkbRoleAttribute instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_role":
            case "role":
            {
                value = instance.m_role;
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
            case "m_role":
            case "role":
            {
                if (value is Havok.hk2018.hkbRoleAttribute.Role castValue)
                {
                    instance.m_role = castValue;
                    return true;
                }
                if (value is short shortValue)
                {
                    instance.m_role = (Havok.hk2018.hkbRoleAttribute.Role)shortValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2018.hkbRoleAttribute.RoleFlags castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is short shortValue)
                {
                    instance.m_flags = (Havok.hk2018.hkbRoleAttribute.RoleFlags)shortValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

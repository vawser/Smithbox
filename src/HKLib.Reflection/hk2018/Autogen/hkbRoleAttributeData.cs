// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbRoleAttributeData : HavokData<hkbRoleAttribute> 
{
    public hkbRoleAttributeData(HavokType type, hkbRoleAttribute instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_role":
            case "role":
            {
                if (instance.m_role is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((short)instance.m_role is TGet shortValue)
                {
                    value = shortValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((short)instance.m_flags is TGet shortValue)
                {
                    value = shortValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_role":
            case "role":
            {
                if (value is hkbRoleAttribute.Role castValue)
                {
                    instance.m_role = castValue;
                    return true;
                }
                if (value is short shortValue)
                {
                    instance.m_role = (hkbRoleAttribute.Role)shortValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (value is hkbRoleAttribute.RoleFlags castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is short shortValue)
                {
                    instance.m_flags = (hkbRoleAttribute.RoleFlags)shortValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

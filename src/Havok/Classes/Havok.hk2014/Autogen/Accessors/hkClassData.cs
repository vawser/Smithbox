// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkClassData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkClass>
{
    public hkClassData(Havok.hk2014.hkClass instance) : base(instance) { }

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
            case "m_parent":
            case "parent":
            {
                value = instance.m_parent;
                return true;
            }
            case "m_objectSize":
            case "objectSize":
            {
                value = instance.m_objectSize;
                return true;
            }
            case "m_numImplementedInterfaces":
            case "numImplementedInterfaces":
            {
                value = instance.m_numImplementedInterfaces;
                return true;
            }
            case "m_declaredEnums":
            case "declaredEnums":
            {
                value = instance.m_declaredEnums;
                return true;
            }
            case "m_declaredMembers":
            case "declaredMembers":
            {
                value = instance.m_declaredMembers;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_describedVersion":
            case "describedVersion":
            {
                value = instance.m_describedVersion;
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
            case "m_parent":
            case "parent":
            {
                if (value is null)
                {
                    instance.m_parent = default;
                    return true;
                }
                if (value is Havok.hk2014.hkClass castValue)
                {
                    instance.m_parent = castValue;
                    return true;
                }
                return false;
            }
            case "m_objectSize":
            case "objectSize":
            {
                if (value is not int castValue) return false;
                instance.m_objectSize = castValue;
                return true;
            }
            case "m_numImplementedInterfaces":
            case "numImplementedInterfaces":
            {
                if (value is not int castValue) return false;
                instance.m_numImplementedInterfaces = castValue;
                return true;
            }
            case "m_declaredEnums":
            case "declaredEnums":
            {
                if (value is not List<Havok.hk2014.hkClassEnum> castValue) return false;
                instance.m_declaredEnums = castValue;
                return true;
            }
            case "m_declaredMembers":
            case "declaredMembers":
            {
                if (value is not List<Havok.hk2014.hkClassMember> castValue) return false;
                instance.m_declaredMembers = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2014.hkClass.FlagValues castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_flags = (Havok.hk2014.hkClass.FlagValues)uintValue;
                    return true;
                }
                return false;
            }
            case "m_describedVersion":
            case "describedVersion":
            {
                if (value is not int castValue) return false;
                instance.m_describedVersion = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

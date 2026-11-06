// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hctAttributeDescriptionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hctAttributeDescription>
{
    public hctAttributeDescriptionData(Havok.hk2018.hctAttributeDescription instance) : base(instance) { }

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
            case "m_enabledBy":
            case "enabledBy":
            {
                value = instance.m_enabledBy;
                return true;
            }
            case "m_forcedType":
            case "forcedType":
            {
                value = instance.m_forcedType;
                return true;
            }
            case "m_enum":
            case "enum":
            {
                value = instance.m_enum;
                return true;
            }
            case "m_hint":
            case "hint":
            {
                value = instance.m_hint;
                return true;
            }
            case "m_clearHints":
            case "clearHints":
            {
                value = instance.m_clearHints;
                return true;
            }
            case "m_floatScale":
            case "floatScale":
            {
                value = instance.m_floatScale;
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
            case "m_enabledBy":
            case "enabledBy":
            {
                if (value is null)
                {
                    instance.m_enabledBy = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_enabledBy = castValue;
                    return true;
                }
                return false;
            }
            case "m_forcedType":
            case "forcedType":
            {
                if (value is Havok.hk2018.hctAttributeDescription20151.ForcedType castValue)
                {
                    instance.m_forcedType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_forcedType = (Havok.hk2018.hctAttributeDescription20151.ForcedType)sbyteValue;
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
                if (value is Havok.hk2018.hctAttributeDescription.Enum castValue)
                {
                    instance.m_enum = castValue;
                    return true;
                }
                return false;
            }
            case "m_hint":
            case "hint":
            {
                if (value is Havok.hk2018.hctAttributeDescription20151.Hint castValue)
                {
                    instance.m_hint = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_hint = (Havok.hk2018.hctAttributeDescription20151.Hint)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_clearHints":
            case "clearHints":
            {
                if (value is not bool castValue) return false;
                instance.m_clearHints = castValue;
                return true;
            }
            case "m_floatScale":
            case "floatScale":
            {
                if (value is not float castValue) return false;
                instance.m_floatScale = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hctAttributeDescription20151;

namespace HKLib.Reflection.hk2018;

internal class hctAttributeDescriptionData : HavokData<hctAttributeDescription> 
{
    public hctAttributeDescriptionData(HavokType type, hctAttributeDescription instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_enabledBy":
            case "enabledBy":
            {
                if (instance.m_enabledBy is null)
                {
                    return true;
                }
                if (instance.m_enabledBy is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_forcedType":
            case "forcedType":
            {
                if (instance.m_forcedType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_forcedType is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_enum":
            case "enum":
            {
                if (instance.m_enum is null)
                {
                    return true;
                }
                if (instance.m_enum is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_hint":
            case "hint":
            {
                if (instance.m_hint is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_hint is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_clearHints":
            case "clearHints":
            {
                if (instance.m_clearHints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floatScale":
            case "floatScale":
            {
                if (instance.m_floatScale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is ForcedType castValue)
                {
                    instance.m_forcedType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_forcedType = (ForcedType)sbyteValue;
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
                if (value is hctAttributeDescription.Enum castValue)
                {
                    instance.m_enum = castValue;
                    return true;
                }
                return false;
            }
            case "m_hint":
            case "hint":
            {
                if (value is Hint castValue)
                {
                    instance.m_hint = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_hint = (Hint)sbyteValue;
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

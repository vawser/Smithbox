// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbGetHandleOnBoneModifierData : HavokData<hkbGetHandleOnBoneModifier> 
{
    public hkbGetHandleOnBoneModifierData(HavokType type, hkbGetHandleOnBoneModifier instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_enable":
            case "enable":
            {
                if (instance.m_enable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_handleOut":
            case "handleOut":
            {
                if (instance.m_handleOut is null)
                {
                    return true;
                }
                if (instance.m_handleOut is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_localFrameName":
            case "localFrameName":
            {
                if (instance.m_localFrameName is null)
                {
                    return true;
                }
                if (instance.m_localFrameName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_ragdollBoneIndex":
            case "ragdollBoneIndex":
            {
                if (instance.m_ragdollBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_animationBoneIndex":
            case "animationBoneIndex":
            {
                if (instance.m_animationBoneIndex is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
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
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_handleOut":
            case "handleOut":
            {
                if (value is null)
                {
                    instance.m_handleOut = default;
                    return true;
                }
                if (value is hkbHandle castValue)
                {
                    instance.m_handleOut = castValue;
                    return true;
                }
                return false;
            }
            case "m_localFrameName":
            case "localFrameName":
            {
                if (value is null)
                {
                    instance.m_localFrameName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_localFrameName = castValue;
                    return true;
                }
                return false;
            }
            case "m_ragdollBoneIndex":
            case "ragdollBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_ragdollBoneIndex = castValue;
                return true;
            }
            case "m_animationBoneIndex":
            case "animationBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_animationBoneIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

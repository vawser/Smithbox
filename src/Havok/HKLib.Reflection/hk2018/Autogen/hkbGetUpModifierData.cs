// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbGetUpModifierData : HavokData<hkbGetUpModifier> 
{
    public hkbGetUpModifierData(HavokType type, hkbGetUpModifier instance) : base(type, instance) {}

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
            case "m_groundNormal":
            case "groundNormal":
            {
                if (instance.m_groundNormal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                if (instance.m_duration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_alignWithGroundDuration":
            case "alignWithGroundDuration":
            {
                if (instance.m_alignWithGroundDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rootBoneIndex":
            case "rootBoneIndex":
            {
                if (instance.m_rootBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_otherBoneIndex":
            case "otherBoneIndex":
            {
                if (instance.m_otherBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_anotherBoneIndex":
            case "anotherBoneIndex":
            {
                if (instance.m_anotherBoneIndex is not TGet castValue) return false;
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
            case "m_groundNormal":
            case "groundNormal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_groundNormal = castValue;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                if (value is not float castValue) return false;
                instance.m_duration = castValue;
                return true;
            }
            case "m_alignWithGroundDuration":
            case "alignWithGroundDuration":
            {
                if (value is not float castValue) return false;
                instance.m_alignWithGroundDuration = castValue;
                return true;
            }
            case "m_rootBoneIndex":
            case "rootBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_rootBoneIndex = castValue;
                return true;
            }
            case "m_otherBoneIndex":
            case "otherBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_otherBoneIndex = castValue;
                return true;
            }
            case "m_anotherBoneIndex":
            case "anotherBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_anotherBoneIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

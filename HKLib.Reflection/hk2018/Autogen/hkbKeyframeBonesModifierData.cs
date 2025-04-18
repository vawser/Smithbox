// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbKeyframeBonesModifierData : HavokData<hkbKeyframeBonesModifier> 
{
    public hkbKeyframeBonesModifierData(HavokType type, hkbKeyframeBonesModifier instance) : base(type, instance) {}

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
            case "m_keyframeInfo":
            case "keyframeInfo":
            {
                if (instance.m_keyframeInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_keyframedBonesList":
            case "keyframedBonesList":
            {
                if (instance.m_keyframedBonesList is null)
                {
                    return true;
                }
                if (instance.m_keyframedBonesList is TGet castValue)
                {
                    value = castValue;
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
            case "m_keyframeInfo":
            case "keyframeInfo":
            {
                if (value is not List<hkbKeyframeBonesModifier.KeyframeInfo> castValue) return false;
                instance.m_keyframeInfo = castValue;
                return true;
            }
            case "m_keyframedBonesList":
            case "keyframedBonesList":
            {
                if (value is null)
                {
                    instance.m_keyframedBonesList = default;
                    return true;
                }
                if (value is hkbBoneIndexArray castValue)
                {
                    instance.m_keyframedBonesList = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

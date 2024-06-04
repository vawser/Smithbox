// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbPoweredRagdollControlsModifierData : HavokData<hkbPoweredRagdollControlsModifier> 
{
    public hkbPoweredRagdollControlsModifierData(HavokType type, hkbPoweredRagdollControlsModifier instance) : base(type, instance) {}

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
            case "m_controlData":
            case "controlData":
            {
                if (instance.m_controlData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bones":
            case "bones":
            {
                if (instance.m_bones is null)
                {
                    return true;
                }
                if (instance.m_bones is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_worldFromModelModeData":
            case "worldFromModelModeData":
            {
                if (instance.m_worldFromModelModeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                if (instance.m_boneWeights is null)
                {
                    return true;
                }
                if (instance.m_boneWeights is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_animationBlendFraction":
            case "animationBlendFraction":
            {
                if (instance.m_animationBlendFraction is not TGet castValue) return false;
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
            case "m_controlData":
            case "controlData":
            {
                if (value is not hkbPoweredRagdollControlData castValue) return false;
                instance.m_controlData = castValue;
                return true;
            }
            case "m_bones":
            case "bones":
            {
                if (value is null)
                {
                    instance.m_bones = default;
                    return true;
                }
                if (value is hkbBoneIndexArray castValue)
                {
                    instance.m_bones = castValue;
                    return true;
                }
                return false;
            }
            case "m_worldFromModelModeData":
            case "worldFromModelModeData":
            {
                if (value is not hkbWorldFromModelModeData castValue) return false;
                instance.m_worldFromModelModeData = castValue;
                return true;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                if (value is null)
                {
                    instance.m_boneWeights = default;
                    return true;
                }
                if (value is hkbBoneWeightArray castValue)
                {
                    instance.m_boneWeights = castValue;
                    return true;
                }
                return false;
            }
            case "m_animationBlendFraction":
            case "animationBlendFraction":
            {
                if (value is not float castValue) return false;
                instance.m_animationBlendFraction = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

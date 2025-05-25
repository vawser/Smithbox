// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class CustomBoneFixModifierData : HavokData<CustomBoneFixModifier> 
{
    public CustomBoneFixModifierData(HavokType type, CustomBoneFixModifier instance) : base(type, instance) {}

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
            case "m_targetWS":
            case "targetWS":
            {
                if (instance.m_targetWS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_targetBoneIndex":
            case "targetBoneIndex":
            {
                if (instance.m_targetBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_newTargetGain":
            case "newTargetGain":
            {
                if (instance.m_newTargetGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_onGain":
            case "onGain":
            {
                if (instance.m_onGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_offGain":
            case "offGain":
            {
                if (instance.m_offGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isOk":
            case "isOk":
            {
                if (instance.m_isOk is not TGet castValue) return false;
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
            case "m_targetWS":
            case "targetWS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_targetWS = castValue;
                return true;
            }
            case "m_targetBoneIndex":
            case "targetBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_targetBoneIndex = castValue;
                return true;
            }
            case "m_newTargetGain":
            case "newTargetGain":
            {
                if (value is not float castValue) return false;
                instance.m_newTargetGain = castValue;
                return true;
            }
            case "m_onGain":
            case "onGain":
            {
                if (value is not float castValue) return false;
                instance.m_onGain = castValue;
                return true;
            }
            case "m_offGain":
            case "offGain":
            {
                if (value is not float castValue) return false;
                instance.m_offGain = castValue;
                return true;
            }
            case "m_isOk":
            case "isOk":
            {
                if (value is not bool castValue) return false;
                instance.m_isOk = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

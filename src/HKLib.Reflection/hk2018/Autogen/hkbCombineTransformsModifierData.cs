// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCombineTransformsModifierData : HavokData<hkbCombineTransformsModifier> 
{
    public hkbCombineTransformsModifierData(HavokType type, hkbCombineTransformsModifier instance) : base(type, instance) {}

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
            case "m_translationOut":
            case "translationOut":
            {
                if (instance.m_translationOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rotationOut":
            case "rotationOut":
            {
                if (instance.m_rotationOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_leftTranslation":
            case "leftTranslation":
            {
                if (instance.m_leftTranslation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_leftRotation":
            case "leftRotation":
            {
                if (instance.m_leftRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rightTranslation":
            case "rightTranslation":
            {
                if (instance.m_rightTranslation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rightRotation":
            case "rightRotation":
            {
                if (instance.m_rightRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_invertLeftTransform":
            case "invertLeftTransform":
            {
                if (instance.m_invertLeftTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_invertRightTransform":
            case "invertRightTransform":
            {
                if (instance.m_invertRightTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_invertResult":
            case "invertResult":
            {
                if (instance.m_invertResult is not TGet castValue) return false;
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
            case "m_translationOut":
            case "translationOut":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translationOut = castValue;
                return true;
            }
            case "m_rotationOut":
            case "rotationOut":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotationOut = castValue;
                return true;
            }
            case "m_leftTranslation":
            case "leftTranslation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_leftTranslation = castValue;
                return true;
            }
            case "m_leftRotation":
            case "leftRotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_leftRotation = castValue;
                return true;
            }
            case "m_rightTranslation":
            case "rightTranslation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_rightTranslation = castValue;
                return true;
            }
            case "m_rightRotation":
            case "rightRotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rightRotation = castValue;
                return true;
            }
            case "m_invertLeftTransform":
            case "invertLeftTransform":
            {
                if (value is not bool castValue) return false;
                instance.m_invertLeftTransform = castValue;
                return true;
            }
            case "m_invertRightTransform":
            case "invertRightTransform":
            {
                if (value is not bool castValue) return false;
                instance.m_invertRightTransform = castValue;
                return true;
            }
            case "m_invertResult":
            case "invertResult":
            {
                if (value is not bool castValue) return false;
                instance.m_invertResult = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

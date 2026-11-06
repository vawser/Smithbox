// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbCombineTransformsModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbCombineTransformsModifier>
{
    public hkbCombineTransformsModifierData(Havok.hk2018.hkbCombineTransformsModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_translationOut":
            case "translationOut":
            {
                value = instance.m_translationOut;
                return true;
            }
            case "m_rotationOut":
            case "rotationOut":
            {
                value = instance.m_rotationOut;
                return true;
            }
            case "m_leftTranslation":
            case "leftTranslation":
            {
                value = instance.m_leftTranslation;
                return true;
            }
            case "m_leftRotation":
            case "leftRotation":
            {
                value = instance.m_leftRotation;
                return true;
            }
            case "m_rightTranslation":
            case "rightTranslation":
            {
                value = instance.m_rightTranslation;
                return true;
            }
            case "m_rightRotation":
            case "rightRotation":
            {
                value = instance.m_rightRotation;
                return true;
            }
            case "m_invertLeftTransform":
            case "invertLeftTransform":
            {
                value = instance.m_invertLeftTransform;
                return true;
            }
            case "m_invertRightTransform":
            case "invertRightTransform":
            {
                value = instance.m_invertRightTransform;
                return true;
            }
            case "m_invertResult":
            case "invertResult":
            {
                value = instance.m_invertResult;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
                if (value is Havok.hk2018.hkbVariableBindingSet castValue)
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

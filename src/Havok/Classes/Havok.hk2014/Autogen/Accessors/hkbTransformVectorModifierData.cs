// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbTransformVectorModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbTransformVectorModifier>
{
    public hkbTransformVectorModifierData(Havok.hk2014.hkbTransformVectorModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_rotation":
            case "rotation":
            {
                value = instance.m_rotation;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                value = instance.m_translation;
                return true;
            }
            case "m_vectorIn":
            case "vectorIn":
            {
                value = instance.m_vectorIn;
                return true;
            }
            case "m_vectorOut":
            case "vectorOut":
            {
                value = instance.m_vectorOut;
                return true;
            }
            case "m_rotateOnly":
            case "rotateOnly":
            {
                value = instance.m_rotateOnly;
                return true;
            }
            case "m_inverse":
            case "inverse":
            {
                value = instance.m_inverse;
                return true;
            }
            case "m_computeOnActivate":
            case "computeOnActivate":
            {
                value = instance.m_computeOnActivate;
                return true;
            }
            case "m_computeOnModify":
            case "computeOnModify":
            {
                value = instance.m_computeOnModify;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
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
            case "m_rotation":
            case "rotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotation = castValue;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translation = castValue;
                return true;
            }
            case "m_vectorIn":
            case "vectorIn":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_vectorIn = castValue;
                return true;
            }
            case "m_vectorOut":
            case "vectorOut":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_vectorOut = castValue;
                return true;
            }
            case "m_rotateOnly":
            case "rotateOnly":
            {
                if (value is not bool castValue) return false;
                instance.m_rotateOnly = castValue;
                return true;
            }
            case "m_inverse":
            case "inverse":
            {
                if (value is not bool castValue) return false;
                instance.m_inverse = castValue;
                return true;
            }
            case "m_computeOnActivate":
            case "computeOnActivate":
            {
                if (value is not bool castValue) return false;
                instance.m_computeOnActivate = castValue;
                return true;
            }
            case "m_computeOnModify":
            case "computeOnModify":
            {
                if (value is not bool castValue) return false;
                instance.m_computeOnModify = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

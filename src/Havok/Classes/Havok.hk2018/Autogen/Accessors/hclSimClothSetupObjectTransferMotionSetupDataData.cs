// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclSimClothSetupObjectTransferMotionSetupDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclSimClothSetupObject.TransferMotionSetupData>
{
    public hclSimClothSetupObjectTransferMotionSetupDataData(Havok.hk2018.hclSimClothSetupObject.TransferMotionSetupData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transferMotionTransformSetSetup":
            case "transferMotionTransformSetSetup":
            {
                value = instance.m_transferMotionTransformSetSetup;
                return true;
            }
            case "m_transferMotionTransformName":
            case "transferMotionTransformName":
            {
                value = instance.m_transferMotionTransformName;
                return true;
            }
            case "m_transferTranslationMotion":
            case "transferTranslationMotion":
            {
                value = instance.m_transferTranslationMotion;
                return true;
            }
            case "m_minTranslationSpeed":
            case "minTranslationSpeed":
            {
                value = instance.m_minTranslationSpeed;
                return true;
            }
            case "m_maxTranslationSpeed":
            case "maxTranslationSpeed":
            {
                value = instance.m_maxTranslationSpeed;
                return true;
            }
            case "m_minTranslationBlend":
            case "minTranslationBlend":
            {
                value = instance.m_minTranslationBlend;
                return true;
            }
            case "m_maxTranslationBlend":
            case "maxTranslationBlend":
            {
                value = instance.m_maxTranslationBlend;
                return true;
            }
            case "m_transferRotationMotion":
            case "transferRotationMotion":
            {
                value = instance.m_transferRotationMotion;
                return true;
            }
            case "m_minRotationSpeed":
            case "minRotationSpeed":
            {
                value = instance.m_minRotationSpeed;
                return true;
            }
            case "m_maxRotationSpeed":
            case "maxRotationSpeed":
            {
                value = instance.m_maxRotationSpeed;
                return true;
            }
            case "m_minRotationBlend":
            case "minRotationBlend":
            {
                value = instance.m_minRotationBlend;
                return true;
            }
            case "m_maxRotationBlend":
            case "maxRotationBlend":
            {
                value = instance.m_maxRotationBlend;
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
            case "m_transferMotionTransformSetSetup":
            case "transferMotionTransformSetSetup":
            {
                if (value is null)
                {
                    instance.m_transferMotionTransformSetSetup = default;
                    return true;
                }
                if (value is Havok.hk2018.hclTransformSetSetupObject castValue)
                {
                    instance.m_transferMotionTransformSetSetup = castValue;
                    return true;
                }
                return false;
            }
            case "m_transferMotionTransformName":
            case "transferMotionTransformName":
            {
                if (value is null)
                {
                    instance.m_transferMotionTransformName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_transferMotionTransformName = castValue;
                    return true;
                }
                return false;
            }
            case "m_transferTranslationMotion":
            case "transferTranslationMotion":
            {
                if (value is not bool castValue) return false;
                instance.m_transferTranslationMotion = castValue;
                return true;
            }
            case "m_minTranslationSpeed":
            case "minTranslationSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_minTranslationSpeed = castValue;
                return true;
            }
            case "m_maxTranslationSpeed":
            case "maxTranslationSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_maxTranslationSpeed = castValue;
                return true;
            }
            case "m_minTranslationBlend":
            case "minTranslationBlend":
            {
                if (value is not float castValue) return false;
                instance.m_minTranslationBlend = castValue;
                return true;
            }
            case "m_maxTranslationBlend":
            case "maxTranslationBlend":
            {
                if (value is not float castValue) return false;
                instance.m_maxTranslationBlend = castValue;
                return true;
            }
            case "m_transferRotationMotion":
            case "transferRotationMotion":
            {
                if (value is not bool castValue) return false;
                instance.m_transferRotationMotion = castValue;
                return true;
            }
            case "m_minRotationSpeed":
            case "minRotationSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_minRotationSpeed = castValue;
                return true;
            }
            case "m_maxRotationSpeed":
            case "maxRotationSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_maxRotationSpeed = castValue;
                return true;
            }
            case "m_minRotationBlend":
            case "minRotationBlend":
            {
                if (value is not float castValue) return false;
                instance.m_minRotationBlend = castValue;
                return true;
            }
            case "m_maxRotationBlend":
            case "maxRotationBlend":
            {
                if (value is not float castValue) return false;
                instance.m_maxRotationBlend = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

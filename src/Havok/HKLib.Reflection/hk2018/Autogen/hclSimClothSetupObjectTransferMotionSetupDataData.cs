// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSimClothSetupObjectTransferMotionSetupDataData : HavokData<hclSimClothSetupObject.TransferMotionSetupData> 
{
    public hclSimClothSetupObjectTransferMotionSetupDataData(HavokType type, hclSimClothSetupObject.TransferMotionSetupData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transferMotionTransformSetSetup":
            case "transferMotionTransformSetSetup":
            {
                if (instance.m_transferMotionTransformSetSetup is null)
                {
                    return true;
                }
                if (instance.m_transferMotionTransformSetSetup is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_transferMotionTransformName":
            case "transferMotionTransformName":
            {
                if (instance.m_transferMotionTransformName is null)
                {
                    return true;
                }
                if (instance.m_transferMotionTransformName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_transferTranslationMotion":
            case "transferTranslationMotion":
            {
                if (instance.m_transferTranslationMotion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minTranslationSpeed":
            case "minTranslationSpeed":
            {
                if (instance.m_minTranslationSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxTranslationSpeed":
            case "maxTranslationSpeed":
            {
                if (instance.m_maxTranslationSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minTranslationBlend":
            case "minTranslationBlend":
            {
                if (instance.m_minTranslationBlend is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxTranslationBlend":
            case "maxTranslationBlend":
            {
                if (instance.m_maxTranslationBlend is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transferRotationMotion":
            case "transferRotationMotion":
            {
                if (instance.m_transferRotationMotion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minRotationSpeed":
            case "minRotationSpeed":
            {
                if (instance.m_minRotationSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxRotationSpeed":
            case "maxRotationSpeed":
            {
                if (instance.m_maxRotationSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minRotationBlend":
            case "minRotationBlend":
            {
                if (instance.m_minRotationBlend is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxRotationBlend":
            case "maxRotationBlend":
            {
                if (instance.m_maxRotationBlend is not TGet castValue) return false;
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
            case "m_transferMotionTransformSetSetup":
            case "transferMotionTransformSetSetup":
            {
                if (value is null)
                {
                    instance.m_transferMotionTransformSetSetup = default;
                    return true;
                }
                if (value is hclTransformSetSetupObject castValue)
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

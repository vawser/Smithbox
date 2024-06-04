// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVdbTransferMotionDataData : HavokData<hclVdbTransferMotionData> 
{
    public hclVdbTransferMotionDataData(HavokType type, hclVdbTransferMotionData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                if (instance.m_transformSetIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformIndex":
            case "transformIndex":
            {
                if (instance.m_transformIndex is not TGet castValue) return false;
                value = castValue;
                return true;
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
            case "m_overridden":
            case "overridden":
            {
                if (instance.m_overridden is not TGet castValue) return false;
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
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_transformSetIndex = castValue;
                return true;
            }
            case "m_transformIndex":
            case "transformIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_transformIndex = castValue;
                return true;
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
            case "m_overridden":
            case "overridden":
            {
                if (value is not bool castValue) return false;
                instance.m_overridden = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

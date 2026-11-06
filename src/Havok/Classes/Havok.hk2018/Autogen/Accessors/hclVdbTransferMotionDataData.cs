// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVdbTransferMotionDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVdbTransferMotionData>
{
    public hclVdbTransferMotionDataData(Havok.hk2018.hclVdbTransferMotionData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                value = instance.m_transformSetIndex;
                return true;
            }
            case "m_transformIndex":
            case "transformIndex":
            {
                value = instance.m_transformIndex;
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
            case "m_overridden":
            case "overridden":
            {
                value = instance.m_overridden;
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

// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class FSHKPlusFSA_ACL_COMPRESSION_SETTINGSData : Havok.Formats.HavokFieldAccess<Havok.hk2019.FSHKPlus.FSA_ACL_COMPRESSION_SETTINGS>
{
    public FSHKPlusFSA_ACL_COMPRESSION_SETTINGSData(Havok.hk2019.FSHKPlus.FSA_ACL_COMPRESSION_SETTINGS instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_precision":
            case "precision":
            {
                value = instance.m_precision;
                return true;
            }
            case "m_shellDistance":
            case "shellDistance":
            {
                value = instance.m_shellDistance;
                return true;
            }
            case "m_constantRotationThresholdAngle":
            case "constantRotationThresholdAngle":
            {
                value = instance.m_constantRotationThresholdAngle;
                return true;
            }
            case "m_constantTranslationThreshold":
            case "constantTranslationThreshold":
            {
                value = instance.m_constantTranslationThreshold;
                return true;
            }
            case "m_constantScaleThreshold":
            case "constantScaleThreshold":
            {
                value = instance.m_constantScaleThreshold;
                return true;
            }
            case "m_compressionLevel":
            case "compressionLevel":
            {
                value = instance.m_compressionLevel;
                return true;
            }
            case "m_rotationFormat":
            case "rotationFormat":
            {
                value = instance.m_rotationFormat;
                return true;
            }
            case "m_translationFormat":
            case "translationFormat":
            {
                value = instance.m_translationFormat;
                return true;
            }
            case "m_scaleFormat":
            case "scaleFormat":
            {
                value = instance.m_scaleFormat;
                return true;
            }
            case "m_sampleRate":
            case "sampleRate":
            {
                value = instance.m_sampleRate;
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
            case "m_precision":
            case "precision":
            {
                if (value is not float castValue) return false;
                instance.m_precision = castValue;
                return true;
            }
            case "m_shellDistance":
            case "shellDistance":
            {
                if (value is not float castValue) return false;
                instance.m_shellDistance = castValue;
                return true;
            }
            case "m_constantRotationThresholdAngle":
            case "constantRotationThresholdAngle":
            {
                if (value is not float castValue) return false;
                instance.m_constantRotationThresholdAngle = castValue;
                return true;
            }
            case "m_constantTranslationThreshold":
            case "constantTranslationThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_constantTranslationThreshold = castValue;
                return true;
            }
            case "m_constantScaleThreshold":
            case "constantScaleThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_constantScaleThreshold = castValue;
                return true;
            }
            case "m_compressionLevel":
            case "compressionLevel":
            {
                if (value is not int castValue) return false;
                instance.m_compressionLevel = castValue;
                return true;
            }
            case "m_rotationFormat":
            case "rotationFormat":
            {
                if (value is not int castValue) return false;
                instance.m_rotationFormat = castValue;
                return true;
            }
            case "m_translationFormat":
            case "translationFormat":
            {
                if (value is not int castValue) return false;
                instance.m_translationFormat = castValue;
                return true;
            }
            case "m_scaleFormat":
            case "scaleFormat":
            {
                if (value is not int castValue) return false;
                instance.m_scaleFormat = castValue;
                return true;
            }
            case "m_sampleRate":
            case "sampleRate":
            {
                if (value is not float castValue) return false;
                instance.m_sampleRate = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

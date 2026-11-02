// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class FSHKPlusFSA_ACL_COMPRESSION_SETTINGSData : HavokData<HKLib.hk2019.FSHKPlus.FSA_ACL_COMPRESSION_SETTINGS> 
{
    public FSHKPlusFSA_ACL_COMPRESSION_SETTINGSData(HavokType type, HKLib.hk2019.FSHKPlus.FSA_ACL_COMPRESSION_SETTINGS instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_precision":
            case "precision":
            {
                if (instance.m_precision is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shellDistance":
            case "shellDistance":
            {
                if (instance.m_shellDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constantRotationThresholdAngle":
            case "constantRotationThresholdAngle":
            {
                if (instance.m_constantRotationThresholdAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constantTranslationThreshold":
            case "constantTranslationThreshold":
            {
                if (instance.m_constantTranslationThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constantScaleThreshold":
            case "constantScaleThreshold":
            {
                if (instance.m_constantScaleThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_compressionLevel":
            case "compressionLevel":
            {
                if (instance.m_compressionLevel is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rotationFormat":
            case "rotationFormat":
            {
                if (instance.m_rotationFormat is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_translationFormat":
            case "translationFormat":
            {
                if (instance.m_translationFormat is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_scaleFormat":
            case "scaleFormat":
            {
                if (instance.m_scaleFormat is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sampleRate":
            case "sampleRate":
            {
                if (instance.m_sampleRate is not TGet castValue) return false;
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

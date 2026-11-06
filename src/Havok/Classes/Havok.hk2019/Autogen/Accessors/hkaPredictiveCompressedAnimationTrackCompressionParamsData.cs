// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaPredictiveCompressedAnimationTrackCompressionParamsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaPredictiveCompressedAnimation.TrackCompressionParams>
{
    public hkaPredictiveCompressedAnimationTrackCompressionParamsData(Havok.hk2019.hkaPredictiveCompressedAnimation.TrackCompressionParams instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_staticTranslationTolerance":
            case "staticTranslationTolerance":
            {
                value = instance.m_staticTranslationTolerance;
                return true;
            }
            case "m_staticRotationTolerance":
            case "staticRotationTolerance":
            {
                value = instance.m_staticRotationTolerance;
                return true;
            }
            case "m_staticScaleTolerance":
            case "staticScaleTolerance":
            {
                value = instance.m_staticScaleTolerance;
                return true;
            }
            case "m_staticFloatTolerance":
            case "staticFloatTolerance":
            {
                value = instance.m_staticFloatTolerance;
                return true;
            }
            case "m_dynamicTranslationTolerance":
            case "dynamicTranslationTolerance":
            {
                value = instance.m_dynamicTranslationTolerance;
                return true;
            }
            case "m_dynamicRotationTolerance":
            case "dynamicRotationTolerance":
            {
                value = instance.m_dynamicRotationTolerance;
                return true;
            }
            case "m_dynamicScaleTolerance":
            case "dynamicScaleTolerance":
            {
                value = instance.m_dynamicScaleTolerance;
                return true;
            }
            case "m_dynamicFloatTolerance":
            case "dynamicFloatTolerance":
            {
                value = instance.m_dynamicFloatTolerance;
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
            case "m_staticTranslationTolerance":
            case "staticTranslationTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_staticTranslationTolerance = castValue;
                return true;
            }
            case "m_staticRotationTolerance":
            case "staticRotationTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_staticRotationTolerance = castValue;
                return true;
            }
            case "m_staticScaleTolerance":
            case "staticScaleTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_staticScaleTolerance = castValue;
                return true;
            }
            case "m_staticFloatTolerance":
            case "staticFloatTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_staticFloatTolerance = castValue;
                return true;
            }
            case "m_dynamicTranslationTolerance":
            case "dynamicTranslationTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_dynamicTranslationTolerance = castValue;
                return true;
            }
            case "m_dynamicRotationTolerance":
            case "dynamicRotationTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_dynamicRotationTolerance = castValue;
                return true;
            }
            case "m_dynamicScaleTolerance":
            case "dynamicScaleTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_dynamicScaleTolerance = castValue;
                return true;
            }
            case "m_dynamicFloatTolerance":
            case "dynamicFloatTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_dynamicFloatTolerance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

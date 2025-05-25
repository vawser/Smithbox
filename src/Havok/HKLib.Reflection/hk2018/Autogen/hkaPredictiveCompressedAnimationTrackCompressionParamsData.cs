// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaPredictiveCompressedAnimationTrackCompressionParamsData : HavokData<hkaPredictiveCompressedAnimation.TrackCompressionParams> 
{
    public hkaPredictiveCompressedAnimationTrackCompressionParamsData(HavokType type, hkaPredictiveCompressedAnimation.TrackCompressionParams instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_staticTranslationTolerance":
            case "staticTranslationTolerance":
            {
                if (instance.m_staticTranslationTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_staticRotationTolerance":
            case "staticRotationTolerance":
            {
                if (instance.m_staticRotationTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_staticScaleTolerance":
            case "staticScaleTolerance":
            {
                if (instance.m_staticScaleTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_staticFloatTolerance":
            case "staticFloatTolerance":
            {
                if (instance.m_staticFloatTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynamicTranslationTolerance":
            case "dynamicTranslationTolerance":
            {
                if (instance.m_dynamicTranslationTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynamicRotationTolerance":
            case "dynamicRotationTolerance":
            {
                if (instance.m_dynamicRotationTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynamicScaleTolerance":
            case "dynamicScaleTolerance":
            {
                if (instance.m_dynamicScaleTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynamicFloatTolerance":
            case "dynamicFloatTolerance":
            {
                if (instance.m_dynamicFloatTolerance is not TGet castValue) return false;
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

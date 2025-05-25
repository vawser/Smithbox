// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaSplineCompressedAnimationTrackCompressionParamsData : HavokData<hkaSplineCompressedAnimation.TrackCompressionParams> 
{
    public hkaSplineCompressedAnimationTrackCompressionParamsData(HavokType type, hkaSplineCompressedAnimation.TrackCompressionParams instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rotationTolerance":
            case "rotationTolerance":
            {
                if (instance.m_rotationTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_translationTolerance":
            case "translationTolerance":
            {
                if (instance.m_translationTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_scaleTolerance":
            case "scaleTolerance":
            {
                if (instance.m_scaleTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floatingTolerance":
            case "floatingTolerance":
            {
                if (instance.m_floatingTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rotationDegree":
            case "rotationDegree":
            {
                if (instance.m_rotationDegree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_translationDegree":
            case "translationDegree":
            {
                if (instance.m_translationDegree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_scaleDegree":
            case "scaleDegree":
            {
                if (instance.m_scaleDegree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floatingDegree":
            case "floatingDegree":
            {
                if (instance.m_floatingDegree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rotationQuantizationType":
            case "rotationQuantizationType":
            {
                if (instance.m_rotationQuantizationType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_rotationQuantizationType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_translationQuantizationType":
            case "translationQuantizationType":
            {
                if (instance.m_translationQuantizationType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_translationQuantizationType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_scaleQuantizationType":
            case "scaleQuantizationType":
            {
                if (instance.m_scaleQuantizationType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_scaleQuantizationType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_floatQuantizationType":
            case "floatQuantizationType":
            {
                if (instance.m_floatQuantizationType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_floatQuantizationType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_rotationTolerance":
            case "rotationTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_rotationTolerance = castValue;
                return true;
            }
            case "m_translationTolerance":
            case "translationTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_translationTolerance = castValue;
                return true;
            }
            case "m_scaleTolerance":
            case "scaleTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_scaleTolerance = castValue;
                return true;
            }
            case "m_floatingTolerance":
            case "floatingTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_floatingTolerance = castValue;
                return true;
            }
            case "m_rotationDegree":
            case "rotationDegree":
            {
                if (value is not ushort castValue) return false;
                instance.m_rotationDegree = castValue;
                return true;
            }
            case "m_translationDegree":
            case "translationDegree":
            {
                if (value is not ushort castValue) return false;
                instance.m_translationDegree = castValue;
                return true;
            }
            case "m_scaleDegree":
            case "scaleDegree":
            {
                if (value is not ushort castValue) return false;
                instance.m_scaleDegree = castValue;
                return true;
            }
            case "m_floatingDegree":
            case "floatingDegree":
            {
                if (value is not ushort castValue) return false;
                instance.m_floatingDegree = castValue;
                return true;
            }
            case "m_rotationQuantizationType":
            case "rotationQuantizationType":
            {
                if (value is hkaSplineCompressedAnimation.TrackCompressionParams.RotationQuantization castValue)
                {
                    instance.m_rotationQuantizationType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_rotationQuantizationType = (hkaSplineCompressedAnimation.TrackCompressionParams.RotationQuantization)byteValue;
                    return true;
                }
                return false;
            }
            case "m_translationQuantizationType":
            case "translationQuantizationType":
            {
                if (value is hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization castValue)
                {
                    instance.m_translationQuantizationType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_translationQuantizationType = (hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization)byteValue;
                    return true;
                }
                return false;
            }
            case "m_scaleQuantizationType":
            case "scaleQuantizationType":
            {
                if (value is hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization castValue)
                {
                    instance.m_scaleQuantizationType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_scaleQuantizationType = (hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization)byteValue;
                    return true;
                }
                return false;
            }
            case "m_floatQuantizationType":
            case "floatQuantizationType":
            {
                if (value is hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization castValue)
                {
                    instance.m_floatQuantizationType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_floatQuantizationType = (hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

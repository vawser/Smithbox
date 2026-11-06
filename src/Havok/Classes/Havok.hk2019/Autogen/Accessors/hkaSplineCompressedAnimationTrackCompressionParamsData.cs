// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaSplineCompressedAnimationTrackCompressionParamsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaSplineCompressedAnimation.TrackCompressionParams>
{
    public hkaSplineCompressedAnimationTrackCompressionParamsData(Havok.hk2019.hkaSplineCompressedAnimation.TrackCompressionParams instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rotationTolerance":
            case "rotationTolerance":
            {
                value = instance.m_rotationTolerance;
                return true;
            }
            case "m_translationTolerance":
            case "translationTolerance":
            {
                value = instance.m_translationTolerance;
                return true;
            }
            case "m_scaleTolerance":
            case "scaleTolerance":
            {
                value = instance.m_scaleTolerance;
                return true;
            }
            case "m_floatingTolerance":
            case "floatingTolerance":
            {
                value = instance.m_floatingTolerance;
                return true;
            }
            case "m_rotationDegree":
            case "rotationDegree":
            {
                value = instance.m_rotationDegree;
                return true;
            }
            case "m_translationDegree":
            case "translationDegree":
            {
                value = instance.m_translationDegree;
                return true;
            }
            case "m_scaleDegree":
            case "scaleDegree":
            {
                value = instance.m_scaleDegree;
                return true;
            }
            case "m_floatingDegree":
            case "floatingDegree":
            {
                value = instance.m_floatingDegree;
                return true;
            }
            case "m_rotationQuantizationType":
            case "rotationQuantizationType":
            {
                value = instance.m_rotationQuantizationType;
                return true;
            }
            case "m_translationQuantizationType":
            case "translationQuantizationType":
            {
                value = instance.m_translationQuantizationType;
                return true;
            }
            case "m_scaleQuantizationType":
            case "scaleQuantizationType":
            {
                value = instance.m_scaleQuantizationType;
                return true;
            }
            case "m_floatQuantizationType":
            case "floatQuantizationType":
            {
                value = instance.m_floatQuantizationType;
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
                if (value is Havok.hk2019.hkaSplineCompressedAnimation.TrackCompressionParams.RotationQuantization castValue)
                {
                    instance.m_rotationQuantizationType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_rotationQuantizationType = (Havok.hk2019.hkaSplineCompressedAnimation.TrackCompressionParams.RotationQuantization)byteValue;
                    return true;
                }
                return false;
            }
            case "m_translationQuantizationType":
            case "translationQuantizationType":
            {
                if (value is Havok.hk2019.hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization castValue)
                {
                    instance.m_translationQuantizationType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_translationQuantizationType = (Havok.hk2019.hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization)byteValue;
                    return true;
                }
                return false;
            }
            case "m_scaleQuantizationType":
            case "scaleQuantizationType":
            {
                if (value is Havok.hk2019.hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization castValue)
                {
                    instance.m_scaleQuantizationType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_scaleQuantizationType = (Havok.hk2019.hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization)byteValue;
                    return true;
                }
                return false;
            }
            case "m_floatQuantizationType":
            case "floatQuantizationType":
            {
                if (value is Havok.hk2019.hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization castValue)
                {
                    instance.m_floatQuantizationType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_floatQuantizationType = (Havok.hk2019.hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

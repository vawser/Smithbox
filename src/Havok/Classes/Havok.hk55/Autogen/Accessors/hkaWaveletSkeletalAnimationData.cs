// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkaWaveletSkeletalAnimationData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkaWaveletSkeletalAnimation>
{
    public hkaWaveletSkeletalAnimationData(Havok.hk55.hkaWaveletSkeletalAnimation instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                value = instance.m_duration;
                return true;
            }
            case "m_numberOfTransformTracks":
            case "numberOfTransformTracks":
            {
                value = instance.m_numberOfTransformTracks;
                return true;
            }
            case "m_numberOfFloatTracks":
            case "numberOfFloatTracks":
            {
                value = instance.m_numberOfFloatTracks;
                return true;
            }
            case "m_extractedMotion":
            case "extractedMotion":
            {
                value = instance.m_extractedMotion;
                return true;
            }
            case "m_annotationTracks":
            case "annotationTracks":
            {
                value = instance.m_annotationTracks;
                return true;
            }
            case "m_numberOfTracks":
            case "numberOfTracks":
            {
                value = instance.m_numberOfTracks;
                return true;
            }
            case "m_numberOfPoses":
            case "numberOfPoses":
            {
                value = instance.m_numberOfPoses;
                return true;
            }
            case "m_blockSize":
            case "blockSize":
            {
                value = instance.m_blockSize;
                return true;
            }
            case "m_qFormat":
            case "qFormat":
            {
                value = instance.m_qFormat;
                return true;
            }
            case "m_staticMaskIdx":
            case "staticMaskIdx":
            {
                value = instance.m_staticMaskIdx;
                return true;
            }
            case "m_staticDOFsIdx":
            case "staticDOFsIdx":
            {
                value = instance.m_staticDOFsIdx;
                return true;
            }
            case "m_blockIndexIdx":
            case "blockIndexIdx":
            {
                value = instance.m_blockIndexIdx;
                return true;
            }
            case "m_blockIndexSize":
            case "blockIndexSize":
            {
                value = instance.m_blockIndexSize;
                return true;
            }
            case "m_quantizedDataIdx":
            case "quantizedDataIdx":
            {
                value = instance.m_quantizedDataIdx;
                return true;
            }
            case "m_quantizedDataSize":
            case "quantizedDataSize":
            {
                value = instance.m_quantizedDataSize;
                return true;
            }
            case "m_dataBuffer":
            case "dataBuffer":
            {
                value = instance.m_dataBuffer;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk55.hkaSkeletalAnimation.AnimationType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_type = (Havok.hk55.hkaSkeletalAnimation.AnimationType)intValue;
                    return true;
                }
                return false;
            }
            case "m_duration":
            case "duration":
            {
                if (value is not float castValue) return false;
                instance.m_duration = castValue;
                return true;
            }
            case "m_numberOfTransformTracks":
            case "numberOfTransformTracks":
            {
                if (value is not int castValue) return false;
                instance.m_numberOfTransformTracks = castValue;
                return true;
            }
            case "m_numberOfFloatTracks":
            case "numberOfFloatTracks":
            {
                if (value is not int castValue) return false;
                instance.m_numberOfFloatTracks = castValue;
                return true;
            }
            case "m_extractedMotion":
            case "extractedMotion":
            {
                if (value is null)
                {
                    instance.m_extractedMotion = default;
                    return true;
                }
                if (value is Havok.hk55.hkaAnimatedReferenceFrame castValue)
                {
                    instance.m_extractedMotion = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotationTracks":
            case "annotationTracks":
            {
                if (value is not List<Havok.hk55.hkaAnnotationTrack?> castValue) return false;
                instance.m_annotationTracks = castValue;
                return true;
            }
            case "m_numberOfTracks":
            case "numberOfTracks":
            {
                if (value is not int castValue) return false;
                instance.m_numberOfTracks = castValue;
                return true;
            }
            case "m_numberOfPoses":
            case "numberOfPoses":
            {
                if (value is not int castValue) return false;
                instance.m_numberOfPoses = castValue;
                return true;
            }
            case "m_blockSize":
            case "blockSize":
            {
                if (value is not int castValue) return false;
                instance.m_blockSize = castValue;
                return true;
            }
            case "m_qFormat":
            case "qFormat":
            {
                if (value is not Havok.hk55.hkaWaveletSkeletalAnimationQuantizationFormat castValue) return false;
                instance.m_qFormat = castValue;
                return true;
            }
            case "m_staticMaskIdx":
            case "staticMaskIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_staticMaskIdx = castValue;
                return true;
            }
            case "m_staticDOFsIdx":
            case "staticDOFsIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_staticDOFsIdx = castValue;
                return true;
            }
            case "m_blockIndexIdx":
            case "blockIndexIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_blockIndexIdx = castValue;
                return true;
            }
            case "m_blockIndexSize":
            case "blockIndexSize":
            {
                if (value is not uint castValue) return false;
                instance.m_blockIndexSize = castValue;
                return true;
            }
            case "m_quantizedDataIdx":
            case "quantizedDataIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_quantizedDataIdx = castValue;
                return true;
            }
            case "m_quantizedDataSize":
            case "quantizedDataSize":
            {
                if (value is not uint castValue) return false;
                instance.m_quantizedDataSize = castValue;
                return true;
            }
            case "m_dataBuffer":
            case "dataBuffer":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_dataBuffer = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

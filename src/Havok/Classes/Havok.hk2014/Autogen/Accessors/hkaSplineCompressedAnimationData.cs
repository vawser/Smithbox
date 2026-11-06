// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaSplineCompressedAnimationData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaSplineCompressedAnimation>
{
    public hkaSplineCompressedAnimationData(Havok.hk2014.hkaSplineCompressedAnimation instance) : base(instance) { }

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
            case "m_numFrames":
            case "numFrames":
            {
                value = instance.m_numFrames;
                return true;
            }
            case "m_numBlocks":
            case "numBlocks":
            {
                value = instance.m_numBlocks;
                return true;
            }
            case "m_maxFramesPerBlock":
            case "maxFramesPerBlock":
            {
                value = instance.m_maxFramesPerBlock;
                return true;
            }
            case "m_maskAndQuantizationSize":
            case "maskAndQuantizationSize":
            {
                value = instance.m_maskAndQuantizationSize;
                return true;
            }
            case "m_blockDuration":
            case "blockDuration":
            {
                value = instance.m_blockDuration;
                return true;
            }
            case "m_blockInverseDuration":
            case "blockInverseDuration":
            {
                value = instance.m_blockInverseDuration;
                return true;
            }
            case "m_frameDuration":
            case "frameDuration":
            {
                value = instance.m_frameDuration;
                return true;
            }
            case "m_blockOffsets":
            case "blockOffsets":
            {
                value = instance.m_blockOffsets;
                return true;
            }
            case "m_floatBlockOffsets":
            case "floatBlockOffsets":
            {
                value = instance.m_floatBlockOffsets;
                return true;
            }
            case "m_transformOffsets":
            case "transformOffsets":
            {
                value = instance.m_transformOffsets;
                return true;
            }
            case "m_floatOffsets":
            case "floatOffsets":
            {
                value = instance.m_floatOffsets;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_endian":
            case "endian":
            {
                value = instance.m_endian;
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
                if (value is Havok.hk2014.hkaAnimation.AnimationType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_type = (Havok.hk2014.hkaAnimation.AnimationType)intValue;
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
                if (value is Havok.hk2014.hkaAnimatedReferenceFrame castValue)
                {
                    instance.m_extractedMotion = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotationTracks":
            case "annotationTracks":
            {
                if (value is not List<Havok.hk2014.hkaAnnotationTrack> castValue) return false;
                instance.m_annotationTracks = castValue;
                return true;
            }
            case "m_numFrames":
            case "numFrames":
            {
                if (value is not int castValue) return false;
                instance.m_numFrames = castValue;
                return true;
            }
            case "m_numBlocks":
            case "numBlocks":
            {
                if (value is not int castValue) return false;
                instance.m_numBlocks = castValue;
                return true;
            }
            case "m_maxFramesPerBlock":
            case "maxFramesPerBlock":
            {
                if (value is not int castValue) return false;
                instance.m_maxFramesPerBlock = castValue;
                return true;
            }
            case "m_maskAndQuantizationSize":
            case "maskAndQuantizationSize":
            {
                if (value is not int castValue) return false;
                instance.m_maskAndQuantizationSize = castValue;
                return true;
            }
            case "m_blockDuration":
            case "blockDuration":
            {
                if (value is not float castValue) return false;
                instance.m_blockDuration = castValue;
                return true;
            }
            case "m_blockInverseDuration":
            case "blockInverseDuration":
            {
                if (value is not float castValue) return false;
                instance.m_blockInverseDuration = castValue;
                return true;
            }
            case "m_frameDuration":
            case "frameDuration":
            {
                if (value is not float castValue) return false;
                instance.m_frameDuration = castValue;
                return true;
            }
            case "m_blockOffsets":
            case "blockOffsets":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_blockOffsets = castValue;
                return true;
            }
            case "m_floatBlockOffsets":
            case "floatBlockOffsets":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_floatBlockOffsets = castValue;
                return true;
            }
            case "m_transformOffsets":
            case "transformOffsets":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_transformOffsets = castValue;
                return true;
            }
            case "m_floatOffsets":
            case "floatOffsets":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_floatOffsets = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_endian":
            case "endian":
            {
                if (value is not int castValue) return false;
                instance.m_endian = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

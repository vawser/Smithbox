// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaSplineCompressedAnimationData : HavokData<hkaSplineCompressedAnimation> 
{
    public hkaSplineCompressedAnimationData(HavokType type, hkaSplineCompressedAnimation instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_type is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_duration":
            case "duration":
            {
                if (instance.m_duration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numberOfTransformTracks":
            case "numberOfTransformTracks":
            {
                if (instance.m_numberOfTransformTracks is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numberOfFloatTracks":
            case "numberOfFloatTracks":
            {
                if (instance.m_numberOfFloatTracks is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_extractedMotion":
            case "extractedMotion":
            {
                if (instance.m_extractedMotion is null)
                {
                    return true;
                }
                if (instance.m_extractedMotion is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotationTracks":
            case "annotationTracks":
            {
                if (instance.m_annotationTracks is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numFrames":
            case "numFrames":
            {
                if (instance.m_numFrames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numBlocks":
            case "numBlocks":
            {
                if (instance.m_numBlocks is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxFramesPerBlock":
            case "maxFramesPerBlock":
            {
                if (instance.m_maxFramesPerBlock is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maskAndQuantizationSize":
            case "maskAndQuantizationSize":
            {
                if (instance.m_maskAndQuantizationSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blockDuration":
            case "blockDuration":
            {
                if (instance.m_blockDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blockInverseDuration":
            case "blockInverseDuration":
            {
                if (instance.m_blockInverseDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_frameDuration":
            case "frameDuration":
            {
                if (instance.m_frameDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blockOffsets":
            case "blockOffsets":
            {
                if (instance.m_blockOffsets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floatBlockOffsets":
            case "floatBlockOffsets":
            {
                if (instance.m_floatBlockOffsets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformOffsets":
            case "transformOffsets":
            {
                if (instance.m_transformOffsets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floatOffsets":
            case "floatOffsets":
            {
                if (instance.m_floatOffsets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endian":
            case "endian":
            {
                if (instance.m_endian is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is hkaAnimation.AnimationType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_type = (hkaAnimation.AnimationType)intValue;
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
                if (value is hkaAnimatedReferenceFrame castValue)
                {
                    instance.m_extractedMotion = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotationTracks":
            case "annotationTracks":
            {
                if (value is not List<hkaAnnotationTrack> castValue) return false;
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

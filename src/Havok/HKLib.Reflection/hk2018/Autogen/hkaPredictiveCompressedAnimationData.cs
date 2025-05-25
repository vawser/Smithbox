// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaPredictiveCompressedAnimationData : HavokData<hkaPredictiveCompressedAnimation> 
{
    private static readonly System.Reflection.FieldInfo _intArrayOffsetsInfo = typeof(hkaPredictiveCompressedAnimation).GetField("m_intArrayOffsets")!;
    private static readonly System.Reflection.FieldInfo _floatArrayOffsetsInfo = typeof(hkaPredictiveCompressedAnimation).GetField("m_floatArrayOffsets")!;
    public hkaPredictiveCompressedAnimationData(HavokType type, hkaPredictiveCompressedAnimation instance) : base(type, instance) {}

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
            case "m_compressedData":
            case "compressedData":
            {
                if (instance.m_compressedData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_intData":
            case "intData":
            {
                if (instance.m_intData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_intArrayOffsets":
            case "intArrayOffsets":
            {
                if (instance.m_intArrayOffsets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floatData":
            case "floatData":
            {
                if (instance.m_floatData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floatArrayOffsets":
            case "floatArrayOffsets":
            {
                if (instance.m_floatArrayOffsets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numBones":
            case "numBones":
            {
                if (instance.m_numBones is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numFloatSlots":
            case "numFloatSlots":
            {
                if (instance.m_numFloatSlots is not TGet castValue) return false;
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
            case "m_firstFloatBlockScaleAndOffsetIndex":
            case "firstFloatBlockScaleAndOffsetIndex":
            {
                if (instance.m_firstFloatBlockScaleAndOffsetIndex is not TGet castValue) return false;
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
            case "m_compressedData":
            case "compressedData":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_compressedData = castValue;
                return true;
            }
            case "m_intData":
            case "intData":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_intData = castValue;
                return true;
            }
            case "m_intArrayOffsets":
            case "intArrayOffsets":
            {
                if (value is not int[] castValue || castValue.Length != 9) return false;
                try
                {
                    _intArrayOffsetsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_floatData":
            case "floatData":
            {
                if (value is not List<float> castValue) return false;
                instance.m_floatData = castValue;
                return true;
            }
            case "m_floatArrayOffsets":
            case "floatArrayOffsets":
            {
                if (value is not int[] castValue || castValue.Length != 3) return false;
                try
                {
                    _floatArrayOffsetsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_numBones":
            case "numBones":
            {
                if (value is not int castValue) return false;
                instance.m_numBones = castValue;
                return true;
            }
            case "m_numFloatSlots":
            case "numFloatSlots":
            {
                if (value is not int castValue) return false;
                instance.m_numFloatSlots = castValue;
                return true;
            }
            case "m_numFrames":
            case "numFrames":
            {
                if (value is not int castValue) return false;
                instance.m_numFrames = castValue;
                return true;
            }
            case "m_firstFloatBlockScaleAndOffsetIndex":
            case "firstFloatBlockScaleAndOffsetIndex":
            {
                if (value is not int castValue) return false;
                instance.m_firstFloatBlockScaleAndOffsetIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

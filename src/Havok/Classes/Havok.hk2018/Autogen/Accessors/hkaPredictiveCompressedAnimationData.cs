// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaPredictiveCompressedAnimationData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaPredictiveCompressedAnimation>
{
    private static readonly System.Reflection.FieldInfo _intArrayOffsetsInfo = typeof(Havok.hk2018.hkaPredictiveCompressedAnimation).GetField("m_intArrayOffsets")!;
    private static readonly System.Reflection.FieldInfo _floatArrayOffsetsInfo = typeof(Havok.hk2018.hkaPredictiveCompressedAnimation).GetField("m_floatArrayOffsets")!;
    public hkaPredictiveCompressedAnimationData(Havok.hk2018.hkaPredictiveCompressedAnimation instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_compressedData":
            case "compressedData":
            {
                value = instance.m_compressedData;
                return true;
            }
            case "m_intData":
            case "intData":
            {
                value = instance.m_intData;
                return true;
            }
            case "m_intArrayOffsets":
            case "intArrayOffsets":
            {
                value = instance.m_intArrayOffsets;
                return true;
            }
            case "m_floatData":
            case "floatData":
            {
                value = instance.m_floatData;
                return true;
            }
            case "m_floatArrayOffsets":
            case "floatArrayOffsets":
            {
                value = instance.m_floatArrayOffsets;
                return true;
            }
            case "m_numBones":
            case "numBones":
            {
                value = instance.m_numBones;
                return true;
            }
            case "m_numFloatSlots":
            case "numFloatSlots":
            {
                value = instance.m_numFloatSlots;
                return true;
            }
            case "m_numFrames":
            case "numFrames":
            {
                value = instance.m_numFrames;
                return true;
            }
            case "m_firstFloatBlockScaleAndOffsetIndex":
            case "firstFloatBlockScaleAndOffsetIndex":
            {
                value = instance.m_firstFloatBlockScaleAndOffsetIndex;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2018.hkaAnimation.AnimationType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_type = (Havok.hk2018.hkaAnimation.AnimationType)intValue;
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
                if (value is Havok.hk2018.hkaAnimatedReferenceFrame castValue)
                {
                    instance.m_extractedMotion = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotationTracks":
            case "annotationTracks":
            {
                if (value is not List<Havok.hk2018.hkaAnnotationTrack> castValue) return false;
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

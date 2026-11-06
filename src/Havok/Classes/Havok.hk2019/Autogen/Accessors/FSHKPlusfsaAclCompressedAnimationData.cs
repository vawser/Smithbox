// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class FSHKPlusfsaAclCompressedAnimationData : Havok.Formats.HavokFieldAccess<Havok.hk2019.FSHKPlus.fsaAclCompressedAnimation>
{
    public FSHKPlusfsaAclCompressedAnimationData(Havok.hk2019.FSHKPlus.fsaAclCompressedAnimation instance) : base(instance) { }

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
            case "m_isInitialized":
            case "isInitialized":
            {
                value = instance.m_isInitialized;
                return true;
            }
            case "m_pSerializedCompressedTracks":
            case "pSerializedCompressedTracks":
            {
                value = instance.m_pSerializedCompressedTracks;
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
                if (value is Havok.hk2019.hkaAnimation.AnimationType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_type = (Havok.hk2019.hkaAnimation.AnimationType)intValue;
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
                if (value is Havok.hk2019.hkaAnimatedReferenceFrame castValue)
                {
                    instance.m_extractedMotion = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotationTracks":
            case "annotationTracks":
            {
                if (value is not List<Havok.hk2019.hkaAnnotationTrack> castValue) return false;
                instance.m_annotationTracks = castValue;
                return true;
            }
            case "m_isInitialized":
            case "isInitialized":
            {
                if (value is not bool castValue) return false;
                instance.m_isInitialized = castValue;
                return true;
            }
            case "m_pSerializedCompressedTracks":
            case "pSerializedCompressedTracks":
            {
                if (value is null)
                {
                    instance.m_pSerializedCompressedTracks = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_pSerializedCompressedTracks = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

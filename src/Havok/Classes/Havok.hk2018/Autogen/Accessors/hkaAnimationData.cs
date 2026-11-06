// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaAnimationData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaAnimation>
{
    public hkaAnimationData(Havok.hk2018.hkaAnimation instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}

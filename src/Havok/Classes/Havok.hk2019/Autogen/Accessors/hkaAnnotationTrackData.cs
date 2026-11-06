// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaAnnotationTrackData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaAnnotationTrack>
{
    public hkaAnnotationTrackData(Havok.hk2019.hkaAnnotationTrack instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_trackName":
            case "trackName":
            {
                value = instance.m_trackName;
                return true;
            }
            case "m_annotations":
            case "annotations":
            {
                value = instance.m_annotations;
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
            case "m_trackName":
            case "trackName":
            {
                if (value is null)
                {
                    instance.m_trackName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_trackName = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotations":
            case "annotations":
            {
                if (value is not List<Havok.hk2019.hkaAnnotationTrack.Annotation> castValue) return false;
                instance.m_annotations = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

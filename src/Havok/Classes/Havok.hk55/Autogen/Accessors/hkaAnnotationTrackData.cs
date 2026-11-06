// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkaAnnotationTrackData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkaAnnotationTrack>
{
    public hkaAnnotationTrackData(Havok.hk55.hkaAnnotationTrack instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotations":
            case "annotations":
            {
                if (value is not List<Havok.hk55.hkaAnnotationTrack.Annotation> castValue) return false;
                instance.m_annotations = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

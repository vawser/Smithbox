// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaAnnotationTrackAnnotationData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaAnnotationTrack.Annotation>
{
    public hkaAnnotationTrackAnnotationData(Havok.hk2014.hkaAnnotationTrack.Annotation instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_time":
            case "time":
            {
                value = instance.m_time;
                return true;
            }
            case "m_text":
            case "text":
            {
                value = instance.m_text;
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
            case "m_time":
            case "time":
            {
                if (value is not float castValue) return false;
                instance.m_time = castValue;
                return true;
            }
            case "m_text":
            case "text":
            {
                if (value is null)
                {
                    instance.m_text = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_text = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

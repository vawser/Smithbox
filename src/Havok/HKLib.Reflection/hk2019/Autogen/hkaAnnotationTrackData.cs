// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaAnnotationTrackData : HavokData<HKLib.hk2019.hkaAnnotationTrack> 
{
    public hkaAnnotationTrackData(HavokType type, HKLib.hk2019.hkaAnnotationTrack instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_trackName":
            case "trackName":
            {
                if (instance.m_trackName is null)
                {
                    return true;
                }
                if (instance.m_trackName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotations":
            case "annotations":
            {
                if (instance.m_annotations is not TGet castValue) return false;
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
                if (value is not List<HKLib.hk2019.hkaAnnotationTrack.Annotation> castValue) return false;
                instance.m_annotations = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

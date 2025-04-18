// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaAnnotationTrackAnnotationData : HavokData<hkaAnnotationTrack.Annotation> 
{
    public hkaAnnotationTrackAnnotationData(HavokType type, hkaAnnotationTrack.Annotation instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_time":
            case "time":
            {
                if (instance.m_time is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_text":
            case "text":
            {
                if (instance.m_text is null)
                {
                    return true;
                }
                if (instance.m_text is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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

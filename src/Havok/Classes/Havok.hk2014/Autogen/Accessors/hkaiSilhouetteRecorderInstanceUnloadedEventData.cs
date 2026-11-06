// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiSilhouetteRecorderInstanceUnloadedEventData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiSilhouetteRecorderInstanceUnloadedEvent>
{
    public hkaiSilhouetteRecorderInstanceUnloadedEventData(Havok.hk2014.hkaiSilhouetteRecorderInstanceUnloadedEvent instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_eventType":
            case "eventType":
            {
                value = instance.m_eventType;
                return true;
            }
            case "m_sectionUid":
            case "sectionUid":
            {
                value = instance.m_sectionUid;
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
            case "m_eventType":
            case "eventType":
            {
                if (value is Havok.hk2014.hkaiSilhouetteRecorderReplayEvent.ReplayEventType castValue)
                {
                    instance.m_eventType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_eventType = (Havok.hk2014.hkaiSilhouetteRecorderReplayEvent.ReplayEventType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_sectionUid":
            case "sectionUid":
            {
                if (value is not uint castValue) return false;
                instance.m_sectionUid = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

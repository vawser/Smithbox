// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiSilhouetteRecorderReplayEventData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiSilhouetteRecorderReplayEvent>
{
    public hkaiSilhouetteRecorderReplayEventData(Havok.hk2014.hkaiSilhouetteRecorderReplayEvent instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}

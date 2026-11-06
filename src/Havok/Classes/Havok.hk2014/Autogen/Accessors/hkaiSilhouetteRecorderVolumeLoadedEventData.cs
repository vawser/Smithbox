// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiSilhouetteRecorderVolumeLoadedEventData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiSilhouetteRecorderVolumeLoadedEvent>
{
    public hkaiSilhouetteRecorderVolumeLoadedEventData(Havok.hk2014.hkaiSilhouetteRecorderVolumeLoadedEvent instance) : base(instance) { }

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
            case "m_volume":
            case "volume":
            {
                value = instance.m_volume;
                return true;
            }
            case "m_mediator":
            case "mediator":
            {
                value = instance.m_mediator;
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
            case "m_volume":
            case "volume":
            {
                if (value is null)
                {
                    instance.m_volume = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiNavVolumeInstance castValue)
                {
                    instance.m_volume = castValue;
                    return true;
                }
                return false;
            }
            case "m_mediator":
            case "mediator":
            {
                if (value is null)
                {
                    instance.m_mediator = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiNavVolumeMediator castValue)
                {
                    instance.m_mediator = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

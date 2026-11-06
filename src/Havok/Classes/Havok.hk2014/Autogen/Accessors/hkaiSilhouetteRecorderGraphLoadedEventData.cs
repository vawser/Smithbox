// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiSilhouetteRecorderGraphLoadedEventData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiSilhouetteRecorderGraphLoadedEvent>
{
    public hkaiSilhouetteRecorderGraphLoadedEventData(Havok.hk2014.hkaiSilhouetteRecorderGraphLoadedEvent instance) : base(instance) { }

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
            case "m_graph":
            case "graph":
            {
                value = instance.m_graph;
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
            case "m_graph":
            case "graph":
            {
                if (value is null)
                {
                    instance.m_graph = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiDirectedGraphInstance castValue)
                {
                    instance.m_graph = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

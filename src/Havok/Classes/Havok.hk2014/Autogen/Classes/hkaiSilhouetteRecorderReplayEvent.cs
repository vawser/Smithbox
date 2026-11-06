// Automatically Generated

namespace Havok.hk2014;

public class hkaiSilhouetteRecorderReplayEvent : hkReferencedObject
{
    public hkaiSilhouetteRecorderReplayEvent.ReplayEventType m_eventType;


    public enum ReplayEventType : int
    {
        EVENT_CONNECT_TO_WORLD = 0,
        EVENT_INSTANCE_LOADED = 1,
        EVENT_INSTANCE_UNLOADED = 2,
        EVENT_STEP_SILHOUETTES = 3,
        EVENT_VOLUME_LOADED = 4,
        EVENT_VOLUME_UNLOADED = 5,
        EVENT_GRAPH_LOADED = 6,
        EVENT_GRAPH_UNLOADED = 7
    }

}

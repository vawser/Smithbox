// Automatically Generated

namespace HKLib.hk2018;

public class hkbEventRangeData : IHavokObject
{
    public float m_upperBound;

    public hkbEventProperty m_event = new();

    public hkbEventRangeData.EventRangeMode m_eventMode;


    public enum EventRangeMode : int
    {
        EVENT_MODE_SEND_ON_ENTER_RANGE = 0,
        EVENT_MODE_SEND_WHEN_IN_RANGE = 1
    }

}


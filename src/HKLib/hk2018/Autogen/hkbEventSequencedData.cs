// Automatically Generated

namespace HKLib.hk2018;

public class hkbEventSequencedData : hkbSequencedData
{
    public List<hkbEventSequencedData.SequencedEvent> m_events = new();


    public class SequencedEvent : IHavokObject
    {
        public hkbEvent m_event = new();

        public float m_time;

    }


}


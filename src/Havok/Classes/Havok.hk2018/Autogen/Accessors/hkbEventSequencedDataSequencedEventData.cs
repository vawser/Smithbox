// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbEventSequencedDataSequencedEventData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbEventSequencedData.SequencedEvent>
{
    public hkbEventSequencedDataSequencedEventData(Havok.hk2018.hkbEventSequencedData.SequencedEvent instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_event":
            case "event":
            {
                value = instance.m_event;
                return true;
            }
            case "m_time":
            case "time":
            {
                value = instance.m_time;
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
            case "m_event":
            case "event":
            {
                if (value is not Havok.hk2018.hkbEvent castValue) return false;
                instance.m_event = castValue;
                return true;
            }
            case "m_time":
            case "time":
            {
                if (value is not float castValue) return false;
                instance.m_time = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

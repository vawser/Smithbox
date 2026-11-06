// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbEventRangeDataArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbEventRangeDataArray>
{
    public hkbEventRangeDataArrayData(Havok.hk2014.hkbEventRangeDataArray instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_eventData":
            case "eventData":
            {
                value = instance.m_eventData;
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
            case "m_eventData":
            case "eventData":
            {
                if (value is not List<Havok.hk2014.hkbEventRangeData> castValue) return false;
                instance.m_eventData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

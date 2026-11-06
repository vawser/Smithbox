// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbEventRangeDataArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbEventRangeDataArray>
{
    public hkbEventRangeDataArrayData(Havok.hk2018.hkbEventRangeDataArray instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_eventData":
            case "eventData":
            {
                if (value is not List<Havok.hk2018.hkbEventRangeData> castValue) return false;
                instance.m_eventData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

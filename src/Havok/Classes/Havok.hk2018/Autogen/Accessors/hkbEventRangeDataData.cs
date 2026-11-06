// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbEventRangeDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbEventRangeData>
{
    public hkbEventRangeDataData(Havok.hk2018.hkbEventRangeData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_upperBound":
            case "upperBound":
            {
                value = instance.m_upperBound;
                return true;
            }
            case "m_event":
            case "event":
            {
                value = instance.m_event;
                return true;
            }
            case "m_eventMode":
            case "eventMode":
            {
                value = instance.m_eventMode;
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
            case "m_upperBound":
            case "upperBound":
            {
                if (value is not float castValue) return false;
                instance.m_upperBound = castValue;
                return true;
            }
            case "m_event":
            case "event":
            {
                if (value is not Havok.hk2018.hkbEventProperty castValue) return false;
                instance.m_event = castValue;
                return true;
            }
            case "m_eventMode":
            case "eventMode":
            {
                if (value is Havok.hk2018.hkbEventRangeData.EventRangeMode castValue)
                {
                    instance.m_eventMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_eventMode = (Havok.hk2018.hkbEventRangeData.EventRangeMode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

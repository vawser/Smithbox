// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbEventRangeDataData : HavokData<hkbEventRangeData> 
{
    public hkbEventRangeDataData(HavokType type, hkbEventRangeData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_upperBound":
            case "upperBound":
            {
                if (instance.m_upperBound is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_event":
            case "event":
            {
                if (instance.m_event is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_eventMode":
            case "eventMode":
            {
                if (instance.m_eventMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_eventMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is not hkbEventProperty castValue) return false;
                instance.m_event = castValue;
                return true;
            }
            case "m_eventMode":
            case "eventMode":
            {
                if (value is hkbEventRangeData.EventRangeMode castValue)
                {
                    instance.m_eventMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_eventMode = (hkbEventRangeData.EventRangeMode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

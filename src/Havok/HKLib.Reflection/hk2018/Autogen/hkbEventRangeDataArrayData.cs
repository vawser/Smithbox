// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbEventRangeDataArrayData : HavokData<hkbEventRangeDataArray> 
{
    public hkbEventRangeDataArrayData(HavokType type, hkbEventRangeDataArray instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_eventData":
            case "eventData":
            {
                if (instance.m_eventData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_eventData":
            case "eventData":
            {
                if (value is not List<hkbEventRangeData> castValue) return false;
                instance.m_eventData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

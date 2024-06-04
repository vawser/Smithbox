// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbEventsFromRangeModifierInternalStateData : HavokData<hkbEventsFromRangeModifierInternalState> 
{
    public hkbEventsFromRangeModifierInternalStateData(HavokType type, hkbEventsFromRangeModifierInternalState instance) : base(type, instance) {}

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
            case "m_wasActiveInPreviousFrame":
            case "wasActiveInPreviousFrame":
            {
                if (instance.m_wasActiveInPreviousFrame is not TGet castValue) return false;
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
            case "m_wasActiveInPreviousFrame":
            case "wasActiveInPreviousFrame":
            {
                if (value is not List<bool> castValue) return false;
                instance.m_wasActiveInPreviousFrame = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

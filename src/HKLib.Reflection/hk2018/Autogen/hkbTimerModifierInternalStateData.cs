// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbTimerModifierInternalStateData : HavokData<hkbTimerModifierInternalState> 
{
    public hkbTimerModifierInternalStateData(HavokType type, hkbTimerModifierInternalState instance) : base(type, instance) {}

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
            case "m_secondsElapsed":
            case "secondsElapsed":
            {
                if (instance.m_secondsElapsed is not TGet castValue) return false;
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
            case "m_secondsElapsed":
            case "secondsElapsed":
            {
                if (value is not float castValue) return false;
                instance.m_secondsElapsed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

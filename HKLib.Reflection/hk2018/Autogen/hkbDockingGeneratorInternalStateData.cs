// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbDockingGeneratorInternalStateData : HavokData<hkbDockingGeneratorInternalState> 
{
    public hkbDockingGeneratorInternalStateData(HavokType type, hkbDockingGeneratorInternalState instance) : base(type, instance) {}

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
            case "m_localTime":
            case "localTime":
            {
                if (instance.m_localTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_previousLocalTime":
            case "previousLocalTime":
            {
                if (instance.m_previousLocalTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_intervalStartLocalTime":
            case "intervalStartLocalTime":
            {
                if (instance.m_intervalStartLocalTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_intervalEndLocalTime":
            case "intervalEndLocalTime":
            {
                if (instance.m_intervalEndLocalTime is not TGet castValue) return false;
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
            case "m_localTime":
            case "localTime":
            {
                if (value is not float castValue) return false;
                instance.m_localTime = castValue;
                return true;
            }
            case "m_previousLocalTime":
            case "previousLocalTime":
            {
                if (value is not float castValue) return false;
                instance.m_previousLocalTime = castValue;
                return true;
            }
            case "m_intervalStartLocalTime":
            case "intervalStartLocalTime":
            {
                if (value is not float castValue) return false;
                instance.m_intervalStartLocalTime = castValue;
                return true;
            }
            case "m_intervalEndLocalTime":
            case "intervalEndLocalTime":
            {
                if (value is not float castValue) return false;
                instance.m_intervalEndLocalTime = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

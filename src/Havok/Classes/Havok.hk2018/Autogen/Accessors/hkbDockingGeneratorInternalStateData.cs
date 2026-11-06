// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbDockingGeneratorInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbDockingGeneratorInternalState>
{
    public hkbDockingGeneratorInternalStateData(Havok.hk2018.hkbDockingGeneratorInternalState instance) : base(instance) { }

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
            case "m_localTime":
            case "localTime":
            {
                value = instance.m_localTime;
                return true;
            }
            case "m_previousLocalTime":
            case "previousLocalTime":
            {
                value = instance.m_previousLocalTime;
                return true;
            }
            case "m_intervalStartLocalTime":
            case "intervalStartLocalTime":
            {
                value = instance.m_intervalStartLocalTime;
                return true;
            }
            case "m_intervalEndLocalTime":
            case "intervalEndLocalTime":
            {
                value = instance.m_intervalEndLocalTime;
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

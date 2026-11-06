// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbGetUpModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbGetUpModifierInternalState>
{
    public hkbGetUpModifierInternalStateData(Havok.hk2018.hkbGetUpModifierInternalState instance) : base(instance) { }

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
            case "m_timeSinceBegin":
            case "timeSinceBegin":
            {
                value = instance.m_timeSinceBegin;
                return true;
            }
            case "m_timeStep":
            case "timeStep":
            {
                value = instance.m_timeStep;
                return true;
            }
            case "m_initNextModify":
            case "initNextModify":
            {
                value = instance.m_initNextModify;
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
            case "m_timeSinceBegin":
            case "timeSinceBegin":
            {
                if (value is not float castValue) return false;
                instance.m_timeSinceBegin = castValue;
                return true;
            }
            case "m_timeStep":
            case "timeStep":
            {
                if (value is not float castValue) return false;
                instance.m_timeStep = castValue;
                return true;
            }
            case "m_initNextModify":
            case "initNextModify":
            {
                if (value is not bool castValue) return false;
                instance.m_initNextModify = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

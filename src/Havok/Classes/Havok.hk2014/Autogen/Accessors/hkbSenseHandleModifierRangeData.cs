// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbSenseHandleModifierRangeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbSenseHandleModifier.Range>
{
    public hkbSenseHandleModifierRangeData(Havok.hk2014.hkbSenseHandleModifier.Range instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_event":
            case "event":
            {
                value = instance.m_event;
                return true;
            }
            case "m_minDistance":
            case "minDistance":
            {
                value = instance.m_minDistance;
                return true;
            }
            case "m_maxDistance":
            case "maxDistance":
            {
                value = instance.m_maxDistance;
                return true;
            }
            case "m_ignoreHandle":
            case "ignoreHandle":
            {
                value = instance.m_ignoreHandle;
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
            case "m_event":
            case "event":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_event = castValue;
                return true;
            }
            case "m_minDistance":
            case "minDistance":
            {
                if (value is not float castValue) return false;
                instance.m_minDistance = castValue;
                return true;
            }
            case "m_maxDistance":
            case "maxDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maxDistance = castValue;
                return true;
            }
            case "m_ignoreHandle":
            case "ignoreHandle":
            {
                if (value is not bool castValue) return false;
                instance.m_ignoreHandle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

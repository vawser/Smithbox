// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbClipTriggerData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbClipTrigger>
{
    public hkbClipTriggerData(Havok.hk2016.hkbClipTrigger instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_localTime":
            case "localTime":
            {
                value = instance.m_localTime;
                return true;
            }
            case "m_event":
            case "event":
            {
                value = instance.m_event;
                return true;
            }
            case "m_relativeToEndOfClip":
            case "relativeToEndOfClip":
            {
                value = instance.m_relativeToEndOfClip;
                return true;
            }
            case "m_acyclic":
            case "acyclic":
            {
                value = instance.m_acyclic;
                return true;
            }
            case "m_isAnnotation":
            case "isAnnotation":
            {
                value = instance.m_isAnnotation;
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
            case "m_localTime":
            case "localTime":
            {
                if (value is not float castValue) return false;
                instance.m_localTime = castValue;
                return true;
            }
            case "m_event":
            case "event":
            {
                if (value is not Havok.hk2016.hkbEventProperty castValue) return false;
                instance.m_event = castValue;
                return true;
            }
            case "m_relativeToEndOfClip":
            case "relativeToEndOfClip":
            {
                if (value is not bool castValue) return false;
                instance.m_relativeToEndOfClip = castValue;
                return true;
            }
            case "m_acyclic":
            case "acyclic":
            {
                if (value is not bool castValue) return false;
                instance.m_acyclic = castValue;
                return true;
            }
            case "m_isAnnotation":
            case "isAnnotation":
            {
                if (value is not bool castValue) return false;
                instance.m_isAnnotation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

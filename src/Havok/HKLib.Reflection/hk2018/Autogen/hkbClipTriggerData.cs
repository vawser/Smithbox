// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbClipTriggerData : HavokData<hkbClipTrigger> 
{
    public hkbClipTriggerData(HavokType type, hkbClipTrigger instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_localTime":
            case "localTime":
            {
                if (instance.m_localTime is not TGet castValue) return false;
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
            case "m_relativeToEndOfClip":
            case "relativeToEndOfClip":
            {
                if (instance.m_relativeToEndOfClip is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_acyclic":
            case "acyclic":
            {
                if (instance.m_acyclic is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isAnnotation":
            case "isAnnotation":
            {
                if (instance.m_isAnnotation is not TGet castValue) return false;
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
                if (value is not hkbEventProperty castValue) return false;
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

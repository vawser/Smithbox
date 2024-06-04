// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbSenseHandleModifierRangeData : HavokData<hkbSenseHandleModifier.Range> 
{
    public hkbSenseHandleModifierRangeData(HavokType type, hkbSenseHandleModifier.Range instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_event":
            case "event":
            {
                if (instance.m_event is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minDistance":
            case "minDistance":
            {
                if (instance.m_minDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxDistance":
            case "maxDistance":
            {
                if (instance.m_maxDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ignoreHandle":
            case "ignoreHandle":
            {
                if (instance.m_ignoreHandle is not TGet castValue) return false;
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
            case "m_event":
            case "event":
            {
                if (value is not hkbEventProperty castValue) return false;
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

// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkMemoryTrackerData : HavokData<MemoryTracker> 
{
    public hkMemoryTrackerData(HavokType type, MemoryTracker instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_opaque":
            case "opaque":
            {
                if (instance.m_opaque is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_handler":
            case "handler":
            {
                if (instance.m_handler is null)
                {
                    return true;
                }
                if (instance.m_handler is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_opaque":
            case "opaque":
            {
                if (value is not bool castValue) return false;
                instance.m_opaque = castValue;
                return true;
            }
            case "m_handler":
            case "handler":
            {
                if (value is null)
                {
                    instance.m_handler = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_handler = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

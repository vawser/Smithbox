// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkMemoryTrackerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.MemoryTracker>
{
    public hkMemoryTrackerData(Havok.hk2018.hk.MemoryTracker instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_opaque":
            case "opaque":
            {
                value = instance.m_opaque;
                return true;
            }
            case "m_handler":
            case "handler":
            {
                value = instance.m_handler;
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

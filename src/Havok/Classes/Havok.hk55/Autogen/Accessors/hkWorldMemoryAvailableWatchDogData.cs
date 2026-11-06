// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkWorldMemoryAvailableWatchDogData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkWorldMemoryAvailableWatchDog>
{
    public hkWorldMemoryAvailableWatchDogData(Havok.hk55.hkWorldMemoryAvailableWatchDog instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minMemoryAvailable":
            case "minMemoryAvailable":
            {
                value = instance.m_minMemoryAvailable;
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
            case "m_minMemoryAvailable":
            case "minMemoryAvailable":
            {
                if (value is not int castValue) return false;
                instance.m_minMemoryAvailable = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

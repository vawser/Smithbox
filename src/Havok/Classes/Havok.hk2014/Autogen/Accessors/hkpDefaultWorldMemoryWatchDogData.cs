// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpDefaultWorldMemoryWatchDogData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpDefaultWorldMemoryWatchDog>
{
    public hkpDefaultWorldMemoryWatchDogData(Havok.hk2014.hkpDefaultWorldMemoryWatchDog instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_freeHeapMemoryRequested":
            case "freeHeapMemoryRequested":
            {
                value = instance.m_freeHeapMemoryRequested;
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
            case "m_freeHeapMemoryRequested":
            case "freeHeapMemoryRequested":
            {
                if (value is not int castValue) return false;
                instance.m_freeHeapMemoryRequested = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

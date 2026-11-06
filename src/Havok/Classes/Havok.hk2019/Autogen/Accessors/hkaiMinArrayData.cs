// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiMinArrayData<T> : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiMinArray<T>>
{
    public hkaiMinArrayData(Havok.hk2019.hkaiMinArray<T> instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_heap":
            case "heap":
            {
                value = instance.m_heap;
                return true;
            }
            case "m_maxSize":
            case "maxSize":
            {
                value = instance.m_maxSize;
                return true;
            }
            case "m_heapified":
            case "heapified":
            {
                value = instance.m_heapified;
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
            case "m_heap":
            case "heap":
            {
                if (value is not Havok.hk2019.hkMinHeap<Havok.hk2019.hkaiMinArrayEntry<T>> castValue) return false;
                instance.m_heap = castValue;
                return true;
            }
            case "m_maxSize":
            case "maxSize":
            {
                if (value is not int castValue) return false;
                instance.m_maxSize = castValue;
                return true;
            }
            case "m_heapified":
            case "heapified":
            {
                if (value is not bool castValue) return false;
                instance.m_heapified = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

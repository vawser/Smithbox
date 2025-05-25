// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiMinArrayData<T> : HavokData<hkaiMinArray<T>> 
{
    public hkaiMinArrayData(HavokType type, hkaiMinArray<T> instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_heap":
            case "heap":
            {
                if (instance.m_heap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxSize":
            case "maxSize":
            {
                if (instance.m_maxSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_heapified":
            case "heapified":
            {
                if (instance.m_heapified is not TGet castValue) return false;
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
            case "m_heap":
            case "heap":
            {
                if (value is not hkMinHeap<hkaiMinArrayEntry<T>> castValue) return false;
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

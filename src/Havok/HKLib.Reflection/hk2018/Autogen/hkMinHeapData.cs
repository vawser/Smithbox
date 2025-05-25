// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMinHeapData<T> : HavokData<hkMinHeap<T>> 
{
    public hkMinHeapData(HavokType type, hkMinHeap<T> instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_contents":
            case "contents":
            {
                if (instance.m_contents is not TGet castValue) return false;
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
            case "m_contents":
            case "contents":
            {
                if (value is not List<T> castValue) return false;
                instance.m_contents = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

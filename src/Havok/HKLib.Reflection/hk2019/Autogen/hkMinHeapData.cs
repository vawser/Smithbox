// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkMinHeapData<T> : HavokData<HKLib.hk2019.hkMinHeap<T>> 
{
    public hkMinHeapData(HavokType type, HKLib.hk2019.hkMinHeap<T> instance) : base(type, instance) {}

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

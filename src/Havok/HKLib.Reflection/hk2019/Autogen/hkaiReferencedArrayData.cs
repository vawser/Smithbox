// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiReferencedArrayData<T> : HavokData<HKLib.hk2019.hkaiReferencedArray<T>> 
{
    public hkaiReferencedArrayData(HavokType type, HKLib.hk2019.hkaiReferencedArray<T> instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_array":
            case "array":
            {
                if (instance.m_array is not TGet castValue) return false;
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
            case "m_array":
            case "array":
            {
                if (value is not List<T> castValue) return false;
                instance.m_array = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

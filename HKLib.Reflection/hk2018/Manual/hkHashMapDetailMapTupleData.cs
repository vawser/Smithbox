// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkHashMapDetail;

namespace HKLib.Reflection.hk2018;

internal class hkHashMapDetailMapTupleData<KEY, VALUE> : HavokData<MapTuple<KEY, VALUE>>
{
    public hkHashMapDetailMapTupleData(HavokType type, MapTuple<KEY, VALUE> instance) :
        base(type, instance) { }

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_0":
            case "0":
            {
                if (instance.m_0 is null) return true;
                if (instance.m_0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_1":
            case "1":
            {
                if (instance.m_1 is null) return true;
                if (instance.m_1 is not TGet castValue) return false;
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
            case "m_0":
            case "0":
            {
                if (value is null)
                {
                    instance.m_0 = default!;
                    return true;
                }

                if (value is not KEY castValue) return false;
                instance.m_0 = castValue;
                return true;
            }
            case "m_1":
            case "1":
            {
                if (value is null)
                {
                    instance.m_1 = default!;
                    return true;
                }

                if (value is not VALUE castValue) return false;
                instance.m_1 = castValue;
                return true;
            }
            default:
                return false;
        }
    }
}
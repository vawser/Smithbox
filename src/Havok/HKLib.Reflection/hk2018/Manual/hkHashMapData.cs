// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkHashMapDetail;

namespace HKLib.Reflection.hk2018;

internal class hkHashMapData<KEY, VALUE> : HavokData<hkHashMap<KEY, VALUE>>
{
    public hkHashMapData(HavokType type, hkHashMap<KEY, VALUE> instance) : base(type, instance) { }

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_items":
            case "items":
            {
                if (instance.m_items is not TGet castValue) return false;
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
            case "m_items":
            case "items":
            {
                if (value is not List<MapTuple<KEY, VALUE>> castValue) return false;
                instance.m_items = castValue;
                return true;
            }
            default:
                return false;
        }
    }
}
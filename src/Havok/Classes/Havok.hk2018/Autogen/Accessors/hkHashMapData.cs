// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkHashMapData<KEY, VALUE> : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkHashMap<KEY, VALUE>>
{
    public hkHashMapData(Havok.hk2018.hkHashMap<KEY, VALUE> instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_items":
            case "items":
            {
                value = instance.m_items;
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
            case "m_items":
            case "items":
            {
                if (value is not List<Havok.hk2018.hkHashMapDetail.MapTuple<KEY, VALUE>> castValue) return false;
                instance.m_items = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkHashSetData<KEY> : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkHashSet<KEY>>
{
    public hkHashSetData(Havok.hk2018.hkHashSet<KEY> instance) : base(instance) { }

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
                if (value is not List<KEY> castValue) return false;
                instance.m_items = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

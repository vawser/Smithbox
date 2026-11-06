// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkHashBaseData<ITEM> : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkHashBase<ITEM>>
{
    public hkHashBaseData(Havok.hk2016.hkHashBase<ITEM> instance) : base(instance) { }

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
                if (value is not List<ITEM> castValue) return false;
                instance.m_items = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

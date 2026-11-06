// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxEnumData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxEnum>
{
    public hkxEnumData(Havok.hk2014.hkxEnum instance) : base(instance) { }

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
                if (value is not List<Havok.hk2014.hkxEnum.Item> castValue) return false;
                instance.m_items = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

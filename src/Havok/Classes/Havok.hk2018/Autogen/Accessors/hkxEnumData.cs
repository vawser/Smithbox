// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxEnumData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxEnum>
{
    public hkxEnumData(Havok.hk2018.hkxEnum instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_items":
            case "items":
            {
                if (value is not List<Havok.hk2018.hkxEnum.Item> castValue) return false;
                instance.m_items = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

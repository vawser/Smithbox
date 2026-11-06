// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hctAttributeDescriptionEnumData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hctAttributeDescription.Enum>
{
    public hctAttributeDescriptionEnumData(Havok.hk2018.hctAttributeDescription.Enum instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_items":
            case "items":
            {
                if (value is not List<Havok.hk2018.hctAttributeDescription.Enum.Item> castValue) return false;
                instance.m_items = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

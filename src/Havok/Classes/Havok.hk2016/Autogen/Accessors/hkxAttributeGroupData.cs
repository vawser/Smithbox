// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkxAttributeGroupData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkxAttributeGroup>
{
    public hkxAttributeGroupData(Havok.hk2016.hkxAttributeGroup instance) : base(instance) { }

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
            case "m_attributes":
            case "attributes":
            {
                value = instance.m_attributes;
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
            case "m_attributes":
            case "attributes":
            {
                if (value is not List<Havok.hk2016.hkxAttribute> castValue) return false;
                instance.m_attributes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkCustomAttributesData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkCustomAttributes>
{
    public hkCustomAttributesData(Havok.hk2014.hkCustomAttributes instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_attributes":
            case "attributes":
            {
                if (value is not List<Havok.hk2014.hkCustomAttributesAttribute> castValue) return false;
                instance.m_attributes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

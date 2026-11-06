// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxAttributeHolderData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxAttributeHolder>
{
    public hkxAttributeHolderData(Havok.hk2019.hkxAttributeHolder instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_attributeGroups":
            case "attributeGroups":
            {
                value = instance.m_attributeGroups;
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
            case "m_attributeGroups":
            case "attributeGroups":
            {
                if (value is not List<Havok.hk2019.hkxAttributeGroup> castValue) return false;
                instance.m_attributeGroups = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

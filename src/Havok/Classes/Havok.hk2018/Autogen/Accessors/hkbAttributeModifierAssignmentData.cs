// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbAttributeModifierAssignmentData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbAttributeModifier.Assignment>
{
    public hkbAttributeModifierAssignmentData(Havok.hk2018.hkbAttributeModifier.Assignment instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_attributeIndex":
            case "attributeIndex":
            {
                value = instance.m_attributeIndex;
                return true;
            }
            case "m_attributeValue":
            case "attributeValue":
            {
                value = instance.m_attributeValue;
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
            case "m_attributeIndex":
            case "attributeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_attributeIndex = castValue;
                return true;
            }
            case "m_attributeValue":
            case "attributeValue":
            {
                if (value is not float castValue) return false;
                instance.m_attributeValue = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

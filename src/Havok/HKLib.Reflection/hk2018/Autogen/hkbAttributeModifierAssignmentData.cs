// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbAttributeModifierAssignmentData : HavokData<hkbAttributeModifier.Assignment> 
{
    public hkbAttributeModifierAssignmentData(HavokType type, hkbAttributeModifier.Assignment instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_attributeIndex":
            case "attributeIndex":
            {
                if (instance.m_attributeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_attributeValue":
            case "attributeValue":
            {
                if (instance.m_attributeValue is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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

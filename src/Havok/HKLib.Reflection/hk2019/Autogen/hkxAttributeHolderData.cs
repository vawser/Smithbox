// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxAttributeHolderData : HavokData<HKLib.hk2019.hkxAttributeHolder> 
{
    public hkxAttributeHolderData(HavokType type, HKLib.hk2019.hkxAttributeHolder instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_attributeGroups":
            case "attributeGroups":
            {
                if (instance.m_attributeGroups is not TGet castValue) return false;
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
            case "m_attributeGroups":
            case "attributeGroups":
            {
                if (value is not List<HKLib.hk2019.hkxAttributeGroup> castValue) return false;
                instance.m_attributeGroups = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

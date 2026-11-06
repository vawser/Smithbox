// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkxNodeSelectionSetData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkxNodeSelectionSet>
{
    public hkxNodeSelectionSetData(Havok.hk55.hkxNodeSelectionSet instance) : base(instance) { }

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
            case "m_selectedNodes":
            case "selectedNodes":
            {
                value = instance.m_selectedNodes;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
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
                if (value is not List<Havok.hk55.hkxAttributeGroup> castValue) return false;
                instance.m_attributeGroups = castValue;
                return true;
            }
            case "m_selectedNodes":
            case "selectedNodes":
            {
                if (value is not List<Havok.hk55.hkxNode?> castValue) return false;
                instance.m_selectedNodes = castValue;
                return true;
            }
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
            default:
            return false;
        }
    }

}

// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkxMeshUserChannelInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkxMesh.UserChannelInfo>
{
    public hkxMeshUserChannelInfoData(Havok.hk2010.hkxMesh.UserChannelInfo instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_className":
            case "className":
            {
                value = instance.m_className;
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
                if (value is not List<Havok.hk2010.hkxAttributeGroup> castValue) return false;
                instance.m_attributeGroups = castValue;
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
            case "m_className":
            case "className":
            {
                if (value is null)
                {
                    instance.m_className = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_className = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

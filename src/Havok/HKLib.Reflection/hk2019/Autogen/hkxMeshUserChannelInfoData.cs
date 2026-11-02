// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxMeshUserChannelInfoData : HavokData<HKLib.hk2019.hkxMesh.UserChannelInfo> 
{
    public hkxMeshUserChannelInfoData(HavokType type, HKLib.hk2019.hkxMesh.UserChannelInfo instance) : base(type, instance) {}

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
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_className":
            case "className":
            {
                if (instance.m_className is null)
                {
                    return true;
                }
                if (instance.m_className is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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

// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkxMaterialData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkxMaterial>
{
    public hkxMaterialData(Havok.hk55.hkxMaterial instance) : base(instance) { }

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
            case "m_stages":
            case "stages":
            {
                value = instance.m_stages;
                return true;
            }
            case "m_diffuseColor":
            case "diffuseColor":
            {
                value = instance.m_diffuseColor;
                return true;
            }
            case "m_ambientColor":
            case "ambientColor":
            {
                value = instance.m_ambientColor;
                return true;
            }
            case "m_specularColor":
            case "specularColor":
            {
                value = instance.m_specularColor;
                return true;
            }
            case "m_emissiveColor":
            case "emissiveColor":
            {
                value = instance.m_emissiveColor;
                return true;
            }
            case "m_subMaterials":
            case "subMaterials":
            {
                value = instance.m_subMaterials;
                return true;
            }
            case "m_extraData":
            case "extraData":
            {
                value = instance.m_extraData;
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
            case "m_stages":
            case "stages":
            {
                if (value is not List<Havok.hk55.hkxMaterial.TextureStage> castValue) return false;
                instance.m_stages = castValue;
                return true;
            }
            case "m_diffuseColor":
            case "diffuseColor":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_diffuseColor = castValue;
                return true;
            }
            case "m_ambientColor":
            case "ambientColor":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_ambientColor = castValue;
                return true;
            }
            case "m_specularColor":
            case "specularColor":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_specularColor = castValue;
                return true;
            }
            case "m_emissiveColor":
            case "emissiveColor":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_emissiveColor = castValue;
                return true;
            }
            case "m_subMaterials":
            case "subMaterials":
            {
                if (value is not List<Havok.hk55.hkxMaterial?> castValue) return false;
                instance.m_subMaterials = castValue;
                return true;
            }
            case "m_extraData":
            case "extraData":
            {
                if (value is not Havok.hk55.hkVariant castValue) return false;
                instance.m_extraData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

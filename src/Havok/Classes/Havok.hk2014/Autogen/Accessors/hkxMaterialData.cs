// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxMaterialData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxMaterial>
{
    private static readonly System.Reflection.FieldInfo _uvMapScaleInfo = typeof(Havok.hk2014.hkxMaterial).GetField("m_uvMapScale")!;
    private static readonly System.Reflection.FieldInfo _uvMapOffsetInfo = typeof(Havok.hk2014.hkxMaterial).GetField("m_uvMapOffset")!;
    public hkxMaterialData(Havok.hk2014.hkxMaterial instance) : base(instance) { }

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
            case "m_uvMapScale":
            case "uvMapScale":
            {
                value = instance.m_uvMapScale;
                return true;
            }
            case "m_uvMapOffset":
            case "uvMapOffset":
            {
                value = instance.m_uvMapOffset;
                return true;
            }
            case "m_uvMapRotation":
            case "uvMapRotation":
            {
                value = instance.m_uvMapRotation;
                return true;
            }
            case "m_uvMapAlgorithm":
            case "uvMapAlgorithm":
            {
                value = instance.m_uvMapAlgorithm;
                return true;
            }
            case "m_specularMultiplier":
            case "specularMultiplier":
            {
                value = instance.m_specularMultiplier;
                return true;
            }
            case "m_specularExponent":
            case "specularExponent":
            {
                value = instance.m_specularExponent;
                return true;
            }
            case "m_transparency":
            case "transparency":
            {
                value = instance.m_transparency;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                value = instance.m_properties;
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
                if (value is not List<Havok.hk2014.hkxAttributeGroup> castValue) return false;
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
                if (value is not List<Havok.hk2014.hkxMaterial.TextureStage> castValue) return false;
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
                if (value is not List<Havok.hk2014.hkxMaterial?> castValue) return false;
                instance.m_subMaterials = castValue;
                return true;
            }
            case "m_extraData":
            case "extraData":
            {
                if (value is null)
                {
                    instance.m_extraData = default;
                    return true;
                }
                if (value is Havok.hk2014.hkReferencedObject castValue)
                {
                    instance.m_extraData = castValue;
                    return true;
                }
                return false;
            }
            case "m_uvMapScale":
            case "uvMapScale":
            {
                if (value is not float[] castValue || castValue.Length != 2) return false;
                try
                {
                    _uvMapScaleInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_uvMapOffset":
            case "uvMapOffset":
            {
                if (value is not float[] castValue || castValue.Length != 2) return false;
                try
                {
                    _uvMapOffsetInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_uvMapRotation":
            case "uvMapRotation":
            {
                if (value is not float castValue) return false;
                instance.m_uvMapRotation = castValue;
                return true;
            }
            case "m_uvMapAlgorithm":
            case "uvMapAlgorithm":
            {
                if (value is Havok.hk2014.hkxMaterial.UVMappingAlgorithm castValue)
                {
                    instance.m_uvMapAlgorithm = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_uvMapAlgorithm = (Havok.hk2014.hkxMaterial.UVMappingAlgorithm)uintValue;
                    return true;
                }
                return false;
            }
            case "m_specularMultiplier":
            case "specularMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_specularMultiplier = castValue;
                return true;
            }
            case "m_specularExponent":
            case "specularExponent":
            {
                if (value is not float castValue) return false;
                instance.m_specularExponent = castValue;
                return true;
            }
            case "m_transparency":
            case "transparency":
            {
                if (value is Havok.hk2014.hkxMaterial.Transparency castValue)
                {
                    instance.m_transparency = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_transparency = (Havok.hk2014.hkxMaterial.Transparency)byteValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                if (value is not List<Havok.hk2014.hkxMaterial.Property> castValue) return false;
                instance.m_properties = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

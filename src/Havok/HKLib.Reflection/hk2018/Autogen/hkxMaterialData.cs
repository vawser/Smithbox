// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxMaterialData : HavokData<hkxMaterial> 
{
    private static readonly System.Reflection.FieldInfo _uvMapScaleInfo = typeof(hkxMaterial).GetField("m_uvMapScale")!;
    private static readonly System.Reflection.FieldInfo _uvMapOffsetInfo = typeof(hkxMaterial).GetField("m_uvMapOffset")!;
    public hkxMaterialData(HavokType type, hkxMaterial instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_stages":
            case "stages":
            {
                if (instance.m_stages is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_diffuseColor":
            case "diffuseColor":
            {
                if (instance.m_diffuseColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ambientColor":
            case "ambientColor":
            {
                if (instance.m_ambientColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_specularColor":
            case "specularColor":
            {
                if (instance.m_specularColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_emissiveColor":
            case "emissiveColor":
            {
                if (instance.m_emissiveColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_subMaterials":
            case "subMaterials":
            {
                if (instance.m_subMaterials is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_extraData":
            case "extraData":
            {
                if (instance.m_extraData is null)
                {
                    return true;
                }
                if (instance.m_extraData is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_uvMapScale":
            case "uvMapScale":
            {
                if (instance.m_uvMapScale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uvMapOffset":
            case "uvMapOffset":
            {
                if (instance.m_uvMapOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uvMapRotation":
            case "uvMapRotation":
            {
                if (instance.m_uvMapRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uvMapAlgorithm":
            case "uvMapAlgorithm":
            {
                if (instance.m_uvMapAlgorithm is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_uvMapAlgorithm is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_specularMultiplier":
            case "specularMultiplier":
            {
                if (instance.m_specularMultiplier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_specularExponent":
            case "specularExponent":
            {
                if (instance.m_specularExponent is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transparency":
            case "transparency":
            {
                if (instance.m_transparency is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_transparency is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                if (instance.m_properties is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_attributeGroups":
            case "attributeGroups":
            {
                if (value is not List<hkxAttributeGroup> castValue) return false;
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
                if (value is not List<hkxMaterial.TextureStage> castValue) return false;
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
                if (value is not List<hkxMaterial?> castValue) return false;
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
                if (value is hkReferencedObject castValue)
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
                if (value is hkxMaterial.UVMappingAlgorithm castValue)
                {
                    instance.m_uvMapAlgorithm = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_uvMapAlgorithm = (hkxMaterial.UVMappingAlgorithm)uintValue;
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
                if (value is hkxMaterial.Transparency castValue)
                {
                    instance.m_transparency = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_transparency = (hkxMaterial.Transparency)byteValue;
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
                if (value is not List<hkxMaterial.Property> castValue) return false;
                instance.m_properties = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

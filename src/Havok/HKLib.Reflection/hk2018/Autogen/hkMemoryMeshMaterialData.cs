// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMemoryMeshMaterialData : HavokData<hkMemoryMeshMaterial> 
{
    public hkMemoryMeshMaterialData(HavokType type, hkMemoryMeshMaterial instance) : base(type, instance) {}

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
            case "m_materialName":
            case "materialName":
            {
                if (instance.m_materialName is null)
                {
                    return true;
                }
                if (instance.m_materialName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_textures":
            case "textures":
            {
                if (instance.m_textures is not TGet castValue) return false;
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
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tesselationFactor":
            case "tesselationFactor":
            {
                if (instance.m_tesselationFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_displacementAmount":
            case "displacementAmount":
            {
                if (instance.m_displacementAmount is not TGet castValue) return false;
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
            case "m_materialName":
            case "materialName":
            {
                if (value is null)
                {
                    instance.m_materialName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_materialName = castValue;
                    return true;
                }
                return false;
            }
            case "m_textures":
            case "textures":
            {
                if (value is not List<hkMeshTexture?> castValue) return false;
                instance.m_textures = castValue;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_tesselationFactor":
            case "tesselationFactor":
            {
                if (value is not float castValue) return false;
                instance.m_tesselationFactor = castValue;
                return true;
            }
            case "m_displacementAmount":
            case "displacementAmount":
            {
                if (value is not float castValue) return false;
                instance.m_displacementAmount = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

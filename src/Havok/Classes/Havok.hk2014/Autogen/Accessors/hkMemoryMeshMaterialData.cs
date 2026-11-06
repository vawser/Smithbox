// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkMemoryMeshMaterialData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkMemoryMeshMaterial>
{
    public hkMemoryMeshMaterialData(Havok.hk2014.hkMemoryMeshMaterial instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_materialName":
            case "materialName":
            {
                value = instance.m_materialName;
                return true;
            }
            case "m_textures":
            case "textures":
            {
                value = instance.m_textures;
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
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_tesselationFactor":
            case "tesselationFactor":
            {
                value = instance.m_tesselationFactor;
                return true;
            }
            case "m_displacementAmount":
            case "displacementAmount":
            {
                value = instance.m_displacementAmount;
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
                if (value is not List<Havok.hk2014.hkMeshTexture?> castValue) return false;
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

// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkMemoryMeshTextureData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkMemoryMeshTexture>
{
    public hkMemoryMeshTextureData(Havok.hk2019.hkMemoryMeshTexture instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_filename":
            case "filename":
            {
                value = instance.m_filename;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_format":
            case "format":
            {
                value = instance.m_format;
                return true;
            }
            case "m_hasMipMaps":
            case "hasMipMaps":
            {
                value = instance.m_hasMipMaps;
                return true;
            }
            case "m_filterMode":
            case "filterMode":
            {
                value = instance.m_filterMode;
                return true;
            }
            case "m_usageHint":
            case "usageHint":
            {
                value = instance.m_usageHint;
                return true;
            }
            case "m_textureCoordChannel":
            case "textureCoordChannel":
            {
                value = instance.m_textureCoordChannel;
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
            case "m_filename":
            case "filename":
            {
                if (value is null)
                {
                    instance.m_filename = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_filename = castValue;
                    return true;
                }
                return false;
            }
            case "m_data":
            case "data":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_format":
            case "format":
            {
                if (value is Havok.hk2019.hkMeshTexture.Format castValue)
                {
                    instance.m_format = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_format = (Havok.hk2019.hkMeshTexture.Format)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_hasMipMaps":
            case "hasMipMaps":
            {
                if (value is not bool castValue) return false;
                instance.m_hasMipMaps = castValue;
                return true;
            }
            case "m_filterMode":
            case "filterMode":
            {
                if (value is Havok.hk2019.hkMeshTexture.FilterMode castValue)
                {
                    instance.m_filterMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_filterMode = (Havok.hk2019.hkMeshTexture.FilterMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_usageHint":
            case "usageHint":
            {
                if (value is Havok.hk2019.hkMeshTexture.TextureUsageType castValue)
                {
                    instance.m_usageHint = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_usageHint = (Havok.hk2019.hkMeshTexture.TextureUsageType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_textureCoordChannel":
            case "textureCoordChannel":
            {
                if (value is not int castValue) return false;
                instance.m_textureCoordChannel = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

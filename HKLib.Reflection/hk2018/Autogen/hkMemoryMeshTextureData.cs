// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMemoryMeshTextureData : HavokData<hkMemoryMeshTexture> 
{
    public hkMemoryMeshTextureData(HavokType type, hkMemoryMeshTexture instance) : base(type, instance) {}

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
            case "m_filename":
            case "filename":
            {
                if (instance.m_filename is null)
                {
                    return true;
                }
                if (instance.m_filename is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_format":
            case "format":
            {
                if (instance.m_format is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_format is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_hasMipMaps":
            case "hasMipMaps":
            {
                if (instance.m_hasMipMaps is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_filterMode":
            case "filterMode":
            {
                if (instance.m_filterMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_filterMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_usageHint":
            case "usageHint":
            {
                if (instance.m_usageHint is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_usageHint is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_textureCoordChannel":
            case "textureCoordChannel":
            {
                if (instance.m_textureCoordChannel is not TGet castValue) return false;
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
                if (value is hkMeshTexture.Format castValue)
                {
                    instance.m_format = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_format = (hkMeshTexture.Format)sbyteValue;
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
                if (value is hkMeshTexture.FilterMode castValue)
                {
                    instance.m_filterMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_filterMode = (hkMeshTexture.FilterMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_usageHint":
            case "usageHint":
            {
                if (value is hkMeshTexture.TextureUsageType castValue)
                {
                    instance.m_usageHint = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_usageHint = (hkMeshTexture.TextureUsageType)sbyteValue;
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

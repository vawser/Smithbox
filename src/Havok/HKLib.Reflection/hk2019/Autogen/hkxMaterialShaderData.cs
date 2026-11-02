// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxMaterialShaderData : HavokData<HKLib.hk2019.hkxMaterialShader> 
{
    public hkxMaterialShaderData(HavokType type, HKLib.hk2019.hkxMaterialShader instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_type is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_vertexEntryName":
            case "vertexEntryName":
            {
                if (instance.m_vertexEntryName is null)
                {
                    return true;
                }
                if (instance.m_vertexEntryName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_geomEntryName":
            case "geomEntryName":
            {
                if (instance.m_geomEntryName is null)
                {
                    return true;
                }
                if (instance.m_geomEntryName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_pixelEntryName":
            case "pixelEntryName":
            {
                if (instance.m_pixelEntryName is null)
                {
                    return true;
                }
                if (instance.m_pixelEntryName is TGet castValue)
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
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
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
            case "m_type":
            case "type":
            {
                if (value is HKLib.hk2019.hkxMaterialShader.ShaderType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (HKLib.hk2019.hkxMaterialShader.ShaderType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_vertexEntryName":
            case "vertexEntryName":
            {
                if (value is null)
                {
                    instance.m_vertexEntryName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_vertexEntryName = castValue;
                    return true;
                }
                return false;
            }
            case "m_geomEntryName":
            case "geomEntryName":
            {
                if (value is null)
                {
                    instance.m_geomEntryName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_geomEntryName = castValue;
                    return true;
                }
                return false;
            }
            case "m_pixelEntryName":
            case "pixelEntryName":
            {
                if (value is null)
                {
                    instance.m_pixelEntryName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_pixelEntryName = castValue;
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
            default:
            return false;
        }
    }

}

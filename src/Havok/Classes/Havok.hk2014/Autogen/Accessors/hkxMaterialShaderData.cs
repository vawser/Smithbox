// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxMaterialShaderData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxMaterialShader>
{
    public hkxMaterialShaderData(Havok.hk2014.hkxMaterialShader instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_vertexEntryName":
            case "vertexEntryName":
            {
                value = instance.m_vertexEntryName;
                return true;
            }
            case "m_geomEntryName":
            case "geomEntryName":
            {
                value = instance.m_geomEntryName;
                return true;
            }
            case "m_pixelEntryName":
            case "pixelEntryName":
            {
                value = instance.m_pixelEntryName;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
                if (value is Havok.hk2014.hkxMaterialShader.ShaderType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2014.hkxMaterialShader.ShaderType)byteValue;
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

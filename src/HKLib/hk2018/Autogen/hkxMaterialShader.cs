// Automatically Generated

namespace HKLib.hk2018;

public class hkxMaterialShader : hkReferencedObject
{
    public string? m_name;

    public hkxMaterialShader.ShaderType m_type;

    public string? m_vertexEntryName;

    public string? m_geomEntryName;

    public string? m_pixelEntryName;

    public List<byte> m_data = new();


    public enum ShaderType : int
    {
        EFFECT_TYPE_INVALID = 0,
        EFFECT_TYPE_UNKNOWN = 1,
        EFFECT_TYPE_HLSL_INLINE = 2,
        EFFECT_TYPE_CG_INLINE = 3,
        EFFECT_TYPE_HLSL_FILENAME = 4,
        EFFECT_TYPE_CG_FILENAME = 5,
        EFFECT_TYPE_MAX_ID = 6
    }

}


// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxMaterialShaderSetData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxMaterialShaderSet>
{
    public hkxMaterialShaderSetData(Havok.hk2019.hkxMaterialShaderSet instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shaders":
            case "shaders":
            {
                value = instance.m_shaders;
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
            case "m_shaders":
            case "shaders":
            {
                if (value is not List<Havok.hk2019.hkxMaterialShader?> castValue) return false;
                instance.m_shaders = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

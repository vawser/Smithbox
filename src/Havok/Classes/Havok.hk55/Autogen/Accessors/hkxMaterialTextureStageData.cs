// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkxMaterialTextureStageData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkxMaterial.TextureStage>
{
    public hkxMaterialTextureStageData(Havok.hk55.hkxMaterial.TextureStage instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_texture":
            case "texture":
            {
                value = instance.m_texture;
                return true;
            }
            case "m_usageHint":
            case "usageHint":
            {
                value = instance.m_usageHint;
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
            case "m_texture":
            case "texture":
            {
                if (value is not Havok.hk55.hkVariant castValue) return false;
                instance.m_texture = castValue;
                return true;
            }
            case "m_usageHint":
            case "usageHint":
            {
                if (value is not int castValue) return false;
                instance.m_usageHint = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

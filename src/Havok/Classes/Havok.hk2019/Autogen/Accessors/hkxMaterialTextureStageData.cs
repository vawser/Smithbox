// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxMaterialTextureStageData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxMaterial.TextureStage>
{
    public hkxMaterialTextureStageData(Havok.hk2019.hkxMaterial.TextureStage instance) : base(instance) { }

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
            case "m_tcoordChannel":
            case "tcoordChannel":
            {
                value = instance.m_tcoordChannel;
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
                if (value is null)
                {
                    instance.m_texture = default;
                    return true;
                }
                if (value is Havok.hk2019.hkReferencedObject castValue)
                {
                    instance.m_texture = castValue;
                    return true;
                }
                return false;
            }
            case "m_usageHint":
            case "usageHint":
            {
                if (value is Havok.hk2019.hkxMaterial.TextureType castValue)
                {
                    instance.m_usageHint = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_usageHint = (Havok.hk2019.hkxMaterial.TextureType)intValue;
                    return true;
                }
                return false;
            }
            case "m_tcoordChannel":
            case "tcoordChannel":
            {
                if (value is not int castValue) return false;
                instance.m_tcoordChannel = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

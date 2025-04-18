// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxMaterialTextureStageData : HavokData<hkxMaterial.TextureStage> 
{
    public hkxMaterialTextureStageData(HavokType type, hkxMaterial.TextureStage instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_texture":
            case "texture":
            {
                if (instance.m_texture is null)
                {
                    return true;
                }
                if (instance.m_texture is TGet castValue)
                {
                    value = castValue;
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
                if ((int)instance.m_usageHint is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_tcoordChannel":
            case "tcoordChannel":
            {
                if (instance.m_tcoordChannel is not TGet castValue) return false;
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
            case "m_texture":
            case "texture":
            {
                if (value is null)
                {
                    instance.m_texture = default;
                    return true;
                }
                if (value is hkReferencedObject castValue)
                {
                    instance.m_texture = castValue;
                    return true;
                }
                return false;
            }
            case "m_usageHint":
            case "usageHint":
            {
                if (value is hkxMaterial.TextureType castValue)
                {
                    instance.m_usageHint = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_usageHint = (hkxMaterial.TextureType)intValue;
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

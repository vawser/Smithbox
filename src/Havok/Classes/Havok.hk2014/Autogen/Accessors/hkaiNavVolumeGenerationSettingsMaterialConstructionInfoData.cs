// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiNavVolumeGenerationSettingsMaterialConstructionInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiNavVolumeGenerationSettings.MaterialConstructionInfo>
{
    public hkaiNavVolumeGenerationSettingsMaterialConstructionInfoData(Havok.hk2014.hkaiNavVolumeGenerationSettings.MaterialConstructionInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_materialIndex":
            case "materialIndex":
            {
                value = instance.m_materialIndex;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_resolution":
            case "resolution":
            {
                value = instance.m_resolution;
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
            case "m_materialIndex":
            case "materialIndex":
            {
                if (value is not int castValue) return false;
                instance.m_materialIndex = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2014.hkaiNavVolumeGenerationSettings.MaterialFlagsBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_flags = (Havok.hk2014.hkaiNavVolumeGenerationSettings.MaterialFlagsBits)uintValue;
                    return true;
                }
                return false;
            }
            case "m_resolution":
            case "resolution":
            {
                if (value is not int castValue) return false;
                instance.m_resolution = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

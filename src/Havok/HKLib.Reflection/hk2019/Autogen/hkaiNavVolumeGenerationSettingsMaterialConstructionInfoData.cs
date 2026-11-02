// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavVolumeGenerationSettingsMaterialConstructionInfoData : HavokData<HKLib.hk2019.hkaiNavVolumeGenerationSettings.MaterialConstructionInfo> 
{
    public hkaiNavVolumeGenerationSettingsMaterialConstructionInfoData(HavokType type, HKLib.hk2019.hkaiNavVolumeGenerationSettings.MaterialConstructionInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_materialIndex":
            case "materialIndex":
            {
                if (instance.m_materialIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_flags is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_resolution":
            case "resolution":
            {
                if (instance.m_resolution is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkaiNavVolumeGenerationSettings.MaterialFlagsBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_flags = (HKLib.hk2019.hkaiNavVolumeGenerationSettings.MaterialFlagsBits)uintValue;
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

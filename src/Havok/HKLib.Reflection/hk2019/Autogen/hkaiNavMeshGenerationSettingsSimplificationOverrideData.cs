// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSettingsSimplificationOverrideData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Settings.SimplificationOverride> 
{
    public hkaiNavMeshGenerationSettingsSimplificationOverrideData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Settings.SimplificationOverride instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_volume":
            case "volume":
            {
                if (instance.m_volume is null)
                {
                    return true;
                }
                if (instance.m_volume is TGet castValue)
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
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (instance.m_simplificationSettings is not TGet castValue) return false;
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
            case "m_volume":
            case "volume":
            {
                if (value is null)
                {
                    instance.m_volume = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiVolume castValue)
                {
                    instance.m_volume = castValue;
                    return true;
                }
                return false;
            }
            case "m_data":
            case "data":
            {
                if (value is not int castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (value is not HKLib.hk2019.hkaiNavMeshSimplificationUtils.Settings castValue) return false;
                instance.m_simplificationSettings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

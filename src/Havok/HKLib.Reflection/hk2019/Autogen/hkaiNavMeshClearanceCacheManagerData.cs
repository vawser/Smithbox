// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshClearanceCacheManagerData : HavokData<HKLib.hk2019.hkaiNavMeshClearanceCacheManager> 
{
    public hkaiNavMeshClearanceCacheManagerData(HavokType type, HKLib.hk2019.hkaiNavMeshClearanceCacheManager instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_layerDatas":
            case "layerDatas":
            {
                if (instance.m_layerDatas is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_defaultOption":
            case "defaultOption":
            {
                if (instance.m_defaultOption is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_defaultOption is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_layerDatas":
            case "layerDatas":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshClearanceCacheManager.LayerData> castValue) return false;
                instance.m_layerDatas = castValue;
                return true;
            }
            case "m_defaultOption":
            case "defaultOption":
            {
                if (value is HKLib.hk2019.hkaiNavMeshClearanceCacheManager.DefaultCachingOption castValue)
                {
                    instance.m_defaultOption = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_defaultOption = (HKLib.hk2019.hkaiNavMeshClearanceCacheManager.DefaultCachingOption)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}

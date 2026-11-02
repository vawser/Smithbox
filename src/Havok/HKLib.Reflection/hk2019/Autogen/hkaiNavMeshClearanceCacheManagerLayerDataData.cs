// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshClearanceCacheManagerLayerDataData : HavokData<HKLib.hk2019.hkaiNavMeshClearanceCacheManager.LayerData> 
{
    public hkaiNavMeshClearanceCacheManagerLayerDataData(HavokType type, HKLib.hk2019.hkaiNavMeshClearanceCacheManager.LayerData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_registrations":
            case "registrations":
            {
                if (instance.m_registrations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cacheInfos":
            case "cacheInfos":
            {
                if (instance.m_cacheInfos is not TGet castValue) return false;
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
            case "m_registrations":
            case "registrations":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshClearanceCacheManager.Registration> castValue) return false;
                instance.m_registrations = castValue;
                return true;
            }
            case "m_cacheInfos":
            case "cacheInfos":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshClearanceCacheManager.CacheInfo> castValue) return false;
                instance.m_cacheInfos = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

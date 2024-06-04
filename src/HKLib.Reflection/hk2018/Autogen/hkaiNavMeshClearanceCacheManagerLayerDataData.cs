// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshClearanceCacheManagerLayerDataData : HavokData<hkaiNavMeshClearanceCacheManager.LayerData> 
{
    public hkaiNavMeshClearanceCacheManagerLayerDataData(HavokType type, hkaiNavMeshClearanceCacheManager.LayerData instance) : base(type, instance) {}

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
                if (value is not List<hkaiNavMeshClearanceCacheManager.Registration> castValue) return false;
                instance.m_registrations = castValue;
                return true;
            }
            case "m_cacheInfos":
            case "cacheInfos":
            {
                if (value is not List<hkaiNavMeshClearanceCacheManager.CacheInfo> castValue) return false;
                instance.m_cacheInfos = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}

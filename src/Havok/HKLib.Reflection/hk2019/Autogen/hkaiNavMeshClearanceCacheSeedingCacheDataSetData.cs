// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshClearanceCacheSeedingCacheDataSetData : HavokData<HKLib.hk2019.hkaiNavMeshClearanceCacheSeeding.CacheDataSet> 
{
    public hkaiNavMeshClearanceCacheSeedingCacheDataSetData(HavokType type, HKLib.hk2019.hkaiNavMeshClearanceCacheSeeding.CacheDataSet instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cacheDatas":
            case "cacheDatas":
            {
                if (instance.m_cacheDatas is not TGet castValue) return false;
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
            case "m_cacheDatas":
            case "cacheDatas":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshClearanceCacheSeeding.CacheData> castValue) return false;
                instance.m_cacheDatas = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
